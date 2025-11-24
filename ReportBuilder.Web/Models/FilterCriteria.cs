using ReportBuilder.Core;

namespace ReportBuilder.Web.Models;

/// <summary>
/// Represents a single filter criteria row in the filter builder
/// </summary>
public class FilterCriteria
{
    /// <summary>
    /// Unique identifier for this filter row
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The field being filtered
    /// </summary>
    public MetadataField? Field { get; set; }

    /// <summary>
    /// The filter operation to apply
    /// </summary>
    public FilterOperation Operation { get; set; } = FilterOperation.Equals;

    /// <summary>
    /// The value to filter by
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Second value for BETWEEN operation
    /// </summary>
    public string? Value2 { get; set; }

    /// <summary>
    /// Logical operator to combine with next filter (AND/OR)
    /// </summary>
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
}

/// <summary>
/// Logical operators for combining multiple filters
/// </summary>
public enum LogicalOperator
{
    And,
    Or
}

/// <summary>
/// Helper class to get available operators for a field type
/// </summary>
public static class FilterOperatorHelper
{
    /// <summary>
    /// Gets the appropriate filter operations for a given field data type
    /// </summary>
    public static List<FilterOperation> GetOperatorsForFieldType(FieldDataType dataType)
    {
        return dataType switch
        {
            FieldDataType.String or FieldDataType.Email or FieldDataType.Phone or
            FieldDataType.Url or FieldDataType.Textarea or FieldDataType.ComboBox =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.Contains,
                    FilterOperation.StartsWith,
                    FilterOperation.Like,
                    FilterOperation.NotLike,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            FieldDataType.Int or FieldDataType.Double or FieldDataType.Currency or FieldDataType.Percent =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.LessThan,
                    FilterOperation.LessThanOrEqual,
                    FilterOperation.GreaterThan,
                    FilterOperation.GreaterThanOrEqual,
                    FilterOperation.Between,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            FieldDataType.Date or FieldDataType.DateTime or FieldDataType.Time =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.LessThan,
                    FilterOperation.LessThanOrEqual,
                    FilterOperation.GreaterThan,
                    FilterOperation.GreaterThanOrEqual,
                    FilterOperation.Between,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            FieldDataType.Boolean =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            FieldDataType.Picklist or FieldDataType.MultiPicklist =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.In,
                    FilterOperation.NotIn,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            FieldDataType.Id or FieldDataType.Reference =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.In,
                    FilterOperation.NotIn,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                },

            _ =>
                new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                }
        };
    }

    /// <summary>
    /// Gets the SOQL operator string for a filter operation
    /// </summary>
    public static string GetSoqlOperator(FilterOperation operation)
    {
        return operation switch
        {
            FilterOperation.Equals => "=",
            FilterOperation.NotEquals => "!=",
            FilterOperation.LessThan => "<",
            FilterOperation.LessThanOrEqual => "<=",
            FilterOperation.GreaterThan => ">",
            FilterOperation.GreaterThanOrEqual => ">=",
            FilterOperation.Like => "LIKE",
            FilterOperation.NotLike => "NOT LIKE",
            FilterOperation.In => "IN",
            FilterOperation.NotIn => "NOT IN",
            FilterOperation.IsNull => "= NULL",
            FilterOperation.IsNotNull => "!= NULL",
            _ => "="
        };
    }

    /// <summary>
    /// Formats a value for SOQL based on field data type
    /// </summary>
    public static string FormatValueForSoql(MetadataField field, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "NULL";

        return field.DataType switch
        {
            FieldDataType.String or FieldDataType.Email or FieldDataType.Phone or
            FieldDataType.Url or FieldDataType.Textarea or FieldDataType.Picklist or
            FieldDataType.Id or FieldDataType.ComboBox =>
                $"'{value.Replace("'", "\\'")}'",  // Escape single quotes

            FieldDataType.Date =>
                value,  // Should be in YYYY-MM-DD format

            FieldDataType.DateTime =>
                value,  // Should be in ISO format

            FieldDataType.Boolean =>
                value.ToLower() == "true" ? "TRUE" : "FALSE",

            FieldDataType.Int or FieldDataType.Double or FieldDataType.Currency or FieldDataType.Percent =>
                value,  // Numeric values don't need quotes

            _ => $"'{value.Replace("'", "\\'")}'",
        };
    }

    /// <summary>
    /// Gets display name for filter operation
    /// </summary>
    public static string GetOperationDisplayName(FilterOperation operation)
    {
        return operation switch
        {
            FilterOperation.Equals => "Equals",
            FilterOperation.NotEquals => "Not Equals",
            FilterOperation.LessThan => "Less Than",
            FilterOperation.LessThanOrEqual => "Less Than or Equal",
            FilterOperation.GreaterThan => "Greater Than",
            FilterOperation.GreaterThanOrEqual => "Greater Than or Equal",
            FilterOperation.Like => "Like",
            FilterOperation.NotLike => "Not Like",
            FilterOperation.In => "In",
            FilterOperation.NotIn => "Not In",
            FilterOperation.Contains => "Contains",
            FilterOperation.StartsWith => "Starts With",
            FilterOperation.EndsWith => "Ends With",
            FilterOperation.IsNull => "Is Null",
            FilterOperation.IsNotNull => "Is Not Null",
            FilterOperation.Between => "Between",
            _ => operation.ToString()
        };
    }
}
