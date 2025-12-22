using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ReportBuilder.Infrastructure.Services;

/// <summary>
/// Converts SOQL queries to SQL Server T-SQL
/// </summary>
public class SoqlToSqlConverter
{
    private readonly ILogger<SoqlToSqlConverter> _logger;

    public SoqlToSqlConverter(ILogger<SoqlToSqlConverter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Convert SOQL query to SQL
    /// </summary>
    public string ConvertToSql(string soqlQuery)
    {
        try
        {
            _logger.LogInformation("Converting SOQL to SQL: {Query}", soqlQuery);
            
            var sql = soqlQuery;
            
            // Remove extra whitespace and normalize
            sql = Regex.Replace(sql, @"\s+", " ").Trim();
            
            // Extract components
            var selectClause = ExtractClause(sql, "SELECT", new[] { "FROM" });
            var fromClause = ExtractClause(sql, "FROM", new[] { "WHERE", "ORDER BY", "LIMIT", "OFFSET" });
            var whereClause = ExtractClause(sql, "WHERE", new[] { "ORDER BY", "LIMIT", "OFFSET" });
            var orderByClause = ExtractClause(sql, "ORDER BY", new[] { "LIMIT", "OFFSET" });
            var limitClause = ExtractClause(sql, "LIMIT", new[] { "OFFSET" });
            var offsetClause = ExtractClause(sql, "OFFSET", null);
            
            // Build SQL query
            var sqlBuilder = new StringBuilder();
            
            // Handle TOP clause (SQL Server equivalent of LIMIT)
            if (!string.IsNullOrEmpty(limitClause))
            {
                var topCount = limitClause.Trim();
                sqlBuilder.Append($"SELECT TOP ({topCount}) ");
            }
            else if (!string.IsNullOrEmpty(offsetClause))
            {
                // If we have OFFSET, we'll use OFFSET/FETCH instead of TOP
                sqlBuilder.Append("SELECT ");
            }
            else
            {
                sqlBuilder.Append("SELECT ");
            }
            
            // Add SELECT fields
            sqlBuilder.Append(selectClause);
            
            // Add FROM clause
            sqlBuilder.Append($" FROM [{SanitizeTableName(fromClause)}]");
            
            // Add WHERE clause
            if (!string.IsNullOrEmpty(whereClause))
            {
                var convertedWhere = ConvertWhereClause(whereClause);
                sqlBuilder.Append($" WHERE {convertedWhere}");
            }
            
            // Add ORDER BY clause
            if (!string.IsNullOrEmpty(orderByClause))
            {
                sqlBuilder.Append($" ORDER BY {orderByClause}");
            }
            else if (!string.IsNullOrEmpty(offsetClause))
            {
                // OFFSET requires ORDER BY
                sqlBuilder.Append(" ORDER BY (SELECT NULL)");
            }
            
            // Add OFFSET/FETCH (SQL Server paging)
            if (!string.IsNullOrEmpty(offsetClause))
            {
                sqlBuilder.Append($" OFFSET {offsetClause.Trim()} ROWS");
                
                if (!string.IsNullOrEmpty(limitClause))
                {
                    sqlBuilder.Append($" FETCH NEXT {limitClause.Trim()} ROWS ONLY");
                }
            }
            
            var resultSql = sqlBuilder.ToString();
            _logger.LogInformation("Converted SQL: {Query}", resultSql);
            
            return resultSql;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting SOQL to SQL");
            throw new InvalidOperationException("Failed to convert SOQL query to SQL", ex);
        }
    }

    /// <summary>
    /// Extract clause from query
    /// </summary>
    private string ExtractClause(string query, string startKeyword, string[]? endKeywords)
    {
        if (endKeywords == null)
        {
            // No end keywords - match to end of string
            var pattern = $@"{startKeyword}\s+(.+)$";
            var match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
        
        var patternWithEnd = $@"{startKeyword}\s+(.+?)(?=\s+({string.Join("|", endKeywords)})|$)";
        var matchWithEnd = Regex.Match(query, patternWithEnd, RegexOptions.IgnoreCase);
        
        return matchWithEnd.Success ? matchWithEnd.Groups[1].Value.Trim() : string.Empty;
    }

    /// <summary>
    /// Convert SOQL WHERE clause to SQL WHERE clause
    /// </summary>
    private string ConvertWhereClause(string whereClause)
    {
        var converted = whereClause;
        
        // Convert LIKE patterns
        // SOQL: Name LIKE '%Corp%' → SQL: Name LIKE '%Corp%' (same)
        
        // Convert = null to IS NULL
        converted = Regex.Replace(converted, @"=\s*null", "IS NULL", RegexOptions.IgnoreCase);
        
        // Convert != null to IS NOT NULL
        converted = Regex.Replace(converted, @"!=\s*null", "IS NOT NULL", RegexOptions.IgnoreCase);
        converted = Regex.Replace(converted, @"<>\s*null", "IS NOT NULL", RegexOptions.IgnoreCase);
        
        // Convert date literals if needed
        // SOQL uses ISO format which SQL Server understands
        
        return converted;
    }

    /// <summary>
    /// Sanitize table name
    /// </summary>
    private string SanitizeTableName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    /// <summary>
    /// Parse and validate SOQL query structure
    /// </summary>
    public bool ValidateSoqlQuery(string soqlQuery, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        try
        {
            var query = soqlQuery.Trim();
            
            // Must start with SELECT
            if (!query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Query must start with SELECT";
                return false;
            }
            
            // Must have FROM
            if (!Regex.IsMatch(query, @"\bFROM\b", RegexOptions.IgnoreCase))
            {
                errorMessage = "Query must contain FROM clause";
                return false;
            }
            
            // Check for balanced parentheses (if we support subqueries later)
            var openCount = query.Count(c => c == '(');
            var closeCount = query.Count(c => c == ')');
            if (openCount != closeCount)
            {
                errorMessage = "Unbalanced parentheses in query";
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Query validation error: {ex.Message}";
            return false;
        }
    }
}
