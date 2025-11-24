using ReportBuilder.Core;
using ReportBuilder.Web.Models;
using System.Text;

namespace ReportBuilder.Web.Services;

/// <summary>
/// Helper class to generate SOQL queries from selected fields and filters
/// </summary>
public static class SoqlQueryGenerator
{
    /// <summary>
    /// Generates a complete SOQL query from the selected fields and filters
    /// </summary>
    public static string GenerateQuery(
        string objectName,
        List<MetadataField> selectedFields,
        List<FilterCriteria>? filters = null,
        int? limit = null)
    {
        if (string.IsNullOrWhiteSpace(objectName) || selectedFields == null || !selectedFields.Any())
            return string.Empty;

        var query = new StringBuilder();

        // SELECT clause
        var fieldNames = string.Join(", ", selectedFields.Select(f => f.ApiName));
        query.Append($"SELECT {fieldNames}");

        // FROM clause
        query.Append($"\nFROM {objectName}");

        // WHERE clause
        if (filters != null && filters.Any())
        {
            var whereClause = GenerateWhereClause(filters);
            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                query.Append($"\n{whereClause}");
            }
        }

        // LIMIT clause
        if (limit.HasValue && limit.Value > 0)
        {
            query.Append($"\nLIMIT {limit.Value}");
        }

        return query.ToString();
    }

    /// <summary>
    /// Generates the WHERE clause from a list of filters
    /// </summary>
    public static string GenerateWhereClause(List<FilterCriteria> filters)
    {
        if (filters == null || !filters.Any())
            return string.Empty;

        // Filter out incomplete criteria (missing field)
        var validFilters = filters.Where(f => f.Field != null).ToList();
        if (!validFilters.Any())
            return string.Empty;

        var whereBuilder = new StringBuilder("WHERE ");

        for (int i = 0; i < validFilters.Count; i++)
        {
            var filter = validFilters[i];
            var isLast = i == validFilters.Count - 1;

            // Generate the filter condition
            var condition = GenerateFilterCondition(filter);
            if (!string.IsNullOrWhiteSpace(condition))
            {
                whereBuilder.Append(condition);

                // Add logical operator if not the last filter
                if (!isLast)
                {
                    var logicalOp = filter.LogicalOperator == LogicalOperator.And ? " AND " : " OR ";
                    whereBuilder.Append(logicalOp);
                }
            }
        }

        return whereBuilder.ToString();
    }

    /// <summary>
    /// Generates a single filter condition
    /// </summary>
    private static string GenerateFilterCondition(FilterCriteria filter)
    {
        if (filter.Field == null)
            return string.Empty;

        var fieldName = filter.Field.ApiName;
        var operation = filter.Operation;

        // Handle NULL checks (no value needed)
        if (operation == FilterOperation.IsNull)
        {
            return $"{fieldName} = NULL";
        }
        else if (operation == FilterOperation.IsNotNull)
        {
            return $"{fieldName} != NULL";
        }

        // Validate that we have a value for non-null operations
        if (string.IsNullOrWhiteSpace(filter.Value))
            return string.Empty;

        // Handle BETWEEN operation
        if (operation == FilterOperation.Between)
        {
            if (string.IsNullOrWhiteSpace(filter.Value2))
                return string.Empty;

            var value1 = FilterOperatorHelper.FormatValueForSoql(filter.Field, filter.Value);
            var value2 = FilterOperatorHelper.FormatValueForSoql(filter.Field, filter.Value2);
            return $"{fieldName} >= {value1} AND {fieldName} <= {value2}";
        }

        // Handle LIKE operations with wildcards
        string formattedValue;
        if (operation == FilterOperation.Contains)
        {
            formattedValue = $"'%{filter.Value.Replace("'", "\\'")}%'";
            return $"{fieldName} LIKE {formattedValue}";
        }
        else if (operation == FilterOperation.StartsWith)
        {
            formattedValue = $"'{filter.Value.Replace("'", "\\'")}%'";
            return $"{fieldName} LIKE {formattedValue}";
        }
        else if (operation == FilterOperation.EndsWith)
        {
            formattedValue = $"'%{filter.Value.Replace("'", "\\'")}'";
            return $"{fieldName} LIKE {formattedValue}";
        }
        // Handle IN and NOT IN operations
        else if (operation == FilterOperation.In || operation == FilterOperation.NotIn)
        {
            // Split comma-separated values
            var values = filter.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var formattedValues = values.Select(v => FilterOperatorHelper.FormatValueForSoql(filter.Field, v));
            var valueList = string.Join(", ", formattedValues);
            var soqlOp = operation == FilterOperation.In ? "IN" : "NOT IN";
            return $"{fieldName} {soqlOp} ({valueList})";
        }
        else
        {
            // Standard operators (=, !=, <, >, <=, >=, LIKE, NOT LIKE)
            formattedValue = FilterOperatorHelper.FormatValueForSoql(filter.Field, filter.Value);
            var soqlOp = FilterOperatorHelper.GetSoqlOperator(operation);
            return $"{fieldName} {soqlOp} {formattedValue}";
        }
    }
}
