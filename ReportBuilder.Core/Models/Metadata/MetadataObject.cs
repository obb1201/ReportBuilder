namespace ReportBuilder.Core;

/// <summary>
/// Represents a Salesforce object (sObject) with complete metadata
/// </summary>
public class MetadataObject
{
    /// <summary>
    /// API name of the object (e.g., "Account", "Contact")
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the object
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Namespace prefix if applicable
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Indicates if this is a custom object
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// Indicates if this is a standard Salesforce object
    /// </summary>
    public bool IsStandard { get; set; }

    /// <summary>
    /// Base type (typically "sObject")
    /// </summary>
    public string BaseType { get; set; } = "sObject";

    /// <summary>
    /// Object-level capabilities (filterable, sortable, etc.)
    /// </summary>
    public ObjectCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// All fields belonging to this object
    /// </summary>
    public List<MetadataField> Fields { get; set; } = new();

    /// <summary>
    /// Reference relationships (lookup/master-detail)
    /// </summary>
    public List<MetadataRelationship> Relationships { get; set; } = new();

    /// <summary>
    /// Child relationships (one-to-many)
    /// </summary>
    public List<ChildRelationship> ChildRelationships { get; set; } = new();

    /// <summary>
    /// Audit information
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// When this metadata was last synced
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}

/// <summary>
/// Object-level capabilities for filtering, sorting, grouping, and aggregation
/// </summary>
public class ObjectCapabilities
{
    public bool IsFilterable { get; set; } = true;
    public bool IsSortable { get; set; } = true;
    public bool IsGroupable { get; set; } = true;
    public bool IsAggregatable { get; set; } = true;
    public bool IsQueryable { get; set; } = true;
    public bool IsSearchable { get; set; } = true;
}

/// <summary>
/// Standard audit fields present on most Salesforce objects
/// </summary>
public class AuditInfo
{
    public bool HasCreatedById { get; set; }
    public bool HasCreatedDate { get; set; }
    public bool HasLastModifiedById { get; set; }
    public bool HasLastModifiedDate { get; set; }
    public bool HasSystemModstamp { get; set; }
}
