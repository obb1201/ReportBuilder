using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportBuilder.Infrastructure.Data.Entities;

/// <summary>
/// Database entity for MetadataObject
/// </summary>
[Table("MetadataObjects")]
public class MetadataObjectEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string ApiName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Namespace { get; set; }

    public bool IsCustom { get; set; }

    public bool IsStandard { get; set; }

    [MaxLength(50)]
    public string BaseType { get; set; } = "sObject";

    /// <summary>
    /// JSON serialized ObjectCapabilities
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string CapabilitiesJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized AuditInfo
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string AuditJson { get; set; } = string.Empty;

    public DateTime LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<MetadataFieldEntity> Fields { get; set; } = new List<MetadataFieldEntity>();
    public virtual ICollection<MetadataRelationshipEntity> Relationships { get; set; } = new List<MetadataRelationshipEntity>();
    public virtual ICollection<ChildRelationshipEntity> ChildRelationships { get; set; } = new List<ChildRelationshipEntity>();
}

/// <summary>
/// Database entity for MetadataField
/// </summary>
[Table("MetadataFields")]
public class MetadataFieldEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Object")]
    public int ObjectId { get; set; }

    [Required]
    [MaxLength(255)]
    public string ApiName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DataType { get; set; } = string.Empty;

    public int? Length { get; set; }

    public int? Precision { get; set; }

    public int? Scale { get; set; }

    public bool IsNillable { get; set; }

    [MaxLength(255)]
    public string? DefaultValue { get; set; }

    public bool IsCalculated { get; set; }

    public bool IsCustom { get; set; }

    public bool IsRequired { get; set; }

    public bool IsUnique { get; set; }

    public bool IsExternalId { get; set; }

    [MaxLength(255)]
    public string? ReferenceTo { get; set; }

    [MaxLength(255)]
    public string? RelationshipName { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? HelpText { get; set; }

    /// <summary>
    /// JSON serialized FieldCapabilities
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string CapabilitiesJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized List<PicklistValue>
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? PicklistValuesJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual MetadataObjectEntity Object { get; set; } = null!;
}

/// <summary>
/// Database entity for MetadataRelationship (reference relationships)
/// </summary>
[Table("MetadataRelationships")]
public class MetadataRelationshipEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("FromObject")]
    public int FromObjectId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FromField { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ToObject { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string RelationshipName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Cardinality { get; set; } = "ManyToOne";

    [Required]
    [MaxLength(50)]
    public string RelationshipType { get; set; } = "Lookup";

    public bool IsRequired { get; set; }

    public bool IsPolymorphic { get; set; }

    /// <summary>
    /// JSON serialized List<string> of polymorphic targets
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? PolymorphicTargetsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual MetadataObjectEntity FromObject { get; set; } = null!;
}

/// <summary>
/// Database entity for ChildRelationship (one-to-many relationships)
/// </summary>
[Table("ChildRelationships")]
public class ChildRelationshipEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ParentObject")]
    public int ParentObjectId { get; set; }

    [Required]
    [MaxLength(255)]
    public string ChildObject { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ViaForeignKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string RelationshipName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Cardinality { get; set; } = "OneToMany";

    public bool IsCascadeDelete { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual MetadataObjectEntity ParentObject { get; set; } = null!;
}

/// <summary>
/// Database entity for tracking metadata sync status
/// </summary>
[Table("MetadataSyncStatus")]
public class MetadataSyncStatusEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime? LastSyncStarted { get; set; }

    public DateTime? LastSyncCompleted { get; set; }

    public bool IsSuccess { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? ErrorMessage { get; set; }

    public int ObjectsProcessed { get; set; }

    public int FieldsProcessed { get; set; }

    public int RelationshipsProcessed { get; set; }

    public DateTime CreatedAt { get; set; }
}
