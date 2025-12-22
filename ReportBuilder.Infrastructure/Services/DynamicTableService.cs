using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportBuilder.Core.Models.Metadata;

namespace ReportBuilder.Infrastructure.Services;

/// <summary>
/// Dynamically creates SQL tables based on Salesforce metadata
/// </summary>
public class DynamicTableService
{
    private readonly string _connectionString;
    private readonly ILogger<DynamicTableService> _logger;

    public DynamicTableService(IConfiguration configuration, ILogger<DynamicTableService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    /// <summary>
    /// Create a table for a Salesforce object
    /// </summary>
    public async Task<bool> CreateTableForObjectAsync(MetadataObject metadataObject)
    {
        try
        {
            var tableName = SanitizeTableName(metadataObject.ApiName);
            
            // Check if table already exists
            if (await TableExistsAsync(tableName))
            {
                _logger.LogInformation("Table {TableName} already exists", tableName);
                return true;
            }

            var createTableSql = GenerateCreateTableSql(metadataObject);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync();
            
            _logger.LogInformation("Created table {TableName} successfully", tableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table for {ObjectName}", metadataObject.ApiName);
            return false;
        }
    }

    /// <summary>
    /// Generate CREATE TABLE SQL statement
    /// </summary>
    private string GenerateCreateTableSql(MetadataObject metadataObject)
    {
        var tableName = SanitizeTableName(metadataObject.ApiName);
        var sql = new StringBuilder();
        
        sql.AppendLine($"CREATE TABLE [{tableName}] (");
        
        // Add Id column (always present in Salesforce)
        sql.AppendLine("    [Id] NVARCHAR(18) PRIMARY KEY NOT NULL,");
        
        // Add all other fields
        var fields = metadataObject.Fields
            .Where(f => f.ApiName != "Id") // Skip Id, we already added it
            .OrderBy(f => f.ApiName)
            .ToList();
        
        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var columnDef = GenerateColumnDefinition(field);
            
            sql.Append($"    {columnDef}");
            
            if (i < fields.Count - 1)
                sql.AppendLine(",");
            else
                sql.AppendLine();
        }
        
        sql.AppendLine(");");
        
        // Add indexes for common fields
        sql.AppendLine();
        sql.AppendLine($"CREATE INDEX IX_{tableName}_Name ON [{tableName}]([Name]) WHERE [Name] IS NOT NULL;");
        sql.AppendLine($"CREATE INDEX IX_{tableName}_CreatedDate ON [{tableName}]([CreatedDate]);");
        
        return sql.ToString();
    }

    /// <summary>
    /// Generate column definition based on Salesforce field metadata
    /// </summary>
    private string GenerateColumnDefinition(MetadataField field)
    {
        var columnName = SanitizeColumnName(field.ApiName);
        var sqlType = MapSalesforceTypeToSqlType(field);
        var nullable = field.IsRequired ? "NOT NULL" : "NULL";
        
        return $"[{columnName}] {sqlType} {nullable}";
    }

    /// <summary>
    /// Map Salesforce data type to SQL Server data type
    /// </summary>
    private string MapSalesforceTypeToSqlType(MetadataField field)
    {
        return field.DataType switch
        {
            FieldDataType.String => $"NVARCHAR({Math.Min(field.Length ?? 255, 4000)})",
            FieldDataType.Textarea => "NVARCHAR(MAX)",
            FieldDataType.Email => "NVARCHAR(255)",
            FieldDataType.Phone => "NVARCHAR(40)",
            FieldDataType.Url => "NVARCHAR(255)",
            FieldDataType.Picklist => "NVARCHAR(255)",
            FieldDataType.MultiPicklist => "NVARCHAR(4000)",
            FieldDataType.Int => "INT",
            FieldDataType.Double => "DECIMAL(18, 2)",
            FieldDataType.Currency => "DECIMAL(18, 2)",
            FieldDataType.Percent => "DECIMAL(5, 2)",
            FieldDataType.Boolean => "BIT",
            FieldDataType.Date => "DATE",
            FieldDataType.DateTime => "DATETIME2",
            FieldDataType.Time => "TIME",
            FieldDataType.Reference => "NVARCHAR(18)", // Salesforce ID reference
            FieldDataType.Id => "NVARCHAR(18)",
            _ => "NVARCHAR(255)" // Default fallback
        };
    }

    /// <summary>
    /// Check if table exists
    /// </summary>
    private async Task<bool> TableExistsAsync(string tableName)
    {
        var sql = @"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @TableName";
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TableName", tableName);
        
        var count = (int)await command.ExecuteScalarAsync();
        return count > 0;
    }

    /// <summary>
    /// Drop table if exists
    /// </summary>
    public async Task<bool> DropTableAsync(string objectApiName)
    {
        try
        {
            var tableName = SanitizeTableName(objectApiName);
            
            var sql = $"IF OBJECT_ID('[{tableName}]', 'U') IS NOT NULL DROP TABLE [{tableName}];";
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
            
            _logger.LogInformation("Dropped table {TableName}", tableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping table for {ObjectName}", objectApiName);
            return false;
        }
    }

    /// <summary>
    /// Get list of all created tables
    /// </summary>
    public async Task<List<string>> GetCreatedTablesAsync()
    {
        var tables = new List<string>();
        
        var sql = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE' 
            AND TABLE_NAME NOT IN ('MetadataObjects', 'MetadataFields', 'MetadataRelationships', 
                                    'SalesforceObjectData', 'GeneratedDataLog', 'QueryExecutionLog',
                                    '__EFMigrationsHistory')
            ORDER BY TABLE_NAME";
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        
        return tables;
    }

    /// <summary>
    /// Sanitize table name for SQL
    /// </summary>
    private string SanitizeTableName(string name)
    {
        // Remove any characters that aren't letters, numbers, or underscores
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    /// <summary>
    /// Sanitize column name for SQL
    /// </summary>
    private string SanitizeColumnName(string name)
    {
        return SanitizeTableName(name);
    }
}
