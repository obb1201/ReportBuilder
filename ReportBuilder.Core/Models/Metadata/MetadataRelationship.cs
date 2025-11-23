namespace ReportBuilder.Core.Models.Metadata;

/// <summary>
/// Represents a reference relationship (lookup or master-detail) from one object to another
/// </summary>
public class MetadataRelationship
{
    /// <summary>
    /// The field on the source object (e.g., "AccountId")
    /// </summary>
    public string FromField { get; set; } = string.Empty;

    /// <summary>
    /// The API name of the object being referenced (e.g., "Account")
    /// </summary>
    public string ToObject { get; set; } = string.Empty;

    /// <summary>
    /// The relationship name used in SOQL (e.g., "Account")
    /// </summary>
    public string RelationshipName { get; set; } = string.Empty;

    /// <summary>
    /// Cardinality of the relationship
    /// </summary>
    public RelationshipCardinality Cardinality { get; set; } = RelationshipCardinality.ManyToOne;

    /// <summary>
    /// Type of relationship
    /// </summary>
    public RelationshipType RelationshipType { get; set; } = RelationshipType.Lookup;

    /// <summary>
    /// Whether this relationship is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether this is a polymorphic relationship (e.g., Owner can be User or Group)
    /// </summary>
    public bool IsPolymorphic { get; set; }

    /// <summary>
    /// If polymorphic, the possible target objects
    /// </summary>
    public List<string>? PolymorphicTargets { get; set; }
}

/// <summary>
/// Represents a child relationship (one-to-many) from a parent object to child records
/// </summary>
public class ChildRelationship
{
    /// <summary>
    /// The child object API name (e.g., "Contact")
    /// </summary>
    public string ChildObject { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key field on the child object (e.g., "AccountId")
    /// </summary>
    public string ViaForeignKey { get; set; } = string.Empty;

    /// <summary>
    /// The relationship name used in SOQL subqueries (e.g., "Contacts")
    /// </summary>
    public string RelationshipName { get; set; } = string.Empty;

    /// <summary>
    /// Cardinality of the relationship (always one-to-many for child relationships)
    /// </summary>
    public RelationshipCardinality Cardinality { get; set; } = RelationshipCardinality.OneToMany;

    /// <summary>
    /// Whether this is a cascading relationship (deletes cascade)
    /// </summary>
    public bool IsCascadeDelete { get; set; }
}

/// <summary>
/// Cardinality options for relationships
/// </summary>
public enum RelationshipCardinality
{
    OneToOne,
    OneToMany,
    ManyToOne,
    ManyToMany
}

/// <summary>
/// Types of Salesforce relationships
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// Lookup relationship (loosely coupled)
    /// </summary>
    Lookup,

    /// <summary>
    /// Master-detail relationship (tightly coupled, cascade delete)
    /// </summary>
    MasterDetail,

    /// <summary>
    /// External lookup (to external objects)
    /// </summary>
    ExternalLookup,

    /// <summary>
    /// Hierarchical relationship (self-referencing, e.g., User.Manager)
    /// </summary>
    Hierarchical
}
