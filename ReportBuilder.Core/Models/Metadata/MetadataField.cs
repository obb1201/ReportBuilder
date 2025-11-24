namespace ReportBuilder.Core;

/// <summary>
/// Represents a field on a Salesforce object with complete metadata
/// </summary>
public class MetadataField
{
    /// <summary>
    /// API name of the field (e.g., "AnnualRevenue", "FirstName")
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the field
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the field
    /// </summary>
    public FieldDataType DataType { get; set; }

    /// <summary>
    /// Maximum length for string fields
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// Precision for numeric fields (total digits)
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Scale for numeric fields (decimal places)
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Indicates if the field can be null
    /// </summary>
    public bool IsNillable { get; set; }

    /// <summary>
    /// Default value for the field
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Indicates if this is a calculated/formula field
    /// </summary>
    public bool IsCalculated { get; set; }

    /// <summary>
    /// Indicates if this is a custom field
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// Field-level capabilities
    /// </summary>
    public FieldCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Available picklist values (if applicable)
    /// </summary>
    public List<PicklistValue>? PicklistValues { get; set; }

    /// <summary>
    /// Reference to another object (for lookup/reference fields)
    /// </summary>
    public string? ReferenceTo { get; set; }

    /// <summary>
    /// Relationship name (for reference fields)
    /// </summary>
    public string? RelationshipName { get; set; }

    /// <summary>
    /// Help text/description for the field
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Whether this field is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether this field is unique
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Whether this field is an external ID
    /// </summary>
    public bool IsExternalId { get; set; }
}

/// <summary>
/// Data types supported by Salesforce fields
/// </summary>
public enum FieldDataType
{
    String,
    Boolean,
    Int,
    Double,
    Currency,
    Date,
    DateTime,
    Time,
    Id,
    Reference,
    Email,
    Phone,
    Url,
    Textarea,
    Picklist,
    MultiPicklist,
    Percent,
    Address,
    Location,
    Base64,
    AnyType,
    ComboBox
}

/// <summary>
/// Field-level capabilities for filtering, sorting, grouping, and aggregation
/// </summary>
public class FieldCapabilities
{
    /// <summary>
    /// Whether the field can be used in filters
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Whether the field can be used for sorting
    /// </summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>
    /// Whether the field can be used for grouping
    /// </summary>
    public bool IsGroupable { get; set; } = true;

    /// <summary>
    /// Whether the field supports aggregation
    /// </summary>
    public bool IsAggregatable { get; set; } = false;

    /// <summary>
    /// Available filter operations for this field
    /// </summary>
    public List<FilterOperation> FilterOperations { get; set; } = new();

    /// <summary>
    /// Available sort operations for this field
    /// </summary>
    public List<SortOperation> SortOperations { get; set; } = new();

    /// <summary>
    /// Available aggregate functions for this field
    /// </summary>
    public List<AggregateFunction> AggregateFunctions { get; set; } = new();
}

/// <summary>
/// Filter operations available for fields
/// </summary>
public enum FilterOperation
{
    Equals,
    NotEquals,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Like,
    NotLike,
    In,
    NotIn,
    Contains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
    Between
}

/// <summary>
/// Sort operations available for fields
/// </summary>
public enum SortOperation
{
    Ascending,
    Descending
}

/// <summary>
/// Aggregate functions available for numeric/date fields
/// </summary>
public enum AggregateFunction
{
    Count,
    Sum,
    Avg,
    Min,
    Max,
    CountDistinct
}

/// <summary>
/// Represents a picklist value
/// </summary>
public class PicklistValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefaultValue { get; set; }
    public int SortOrder { get; set; }
}
