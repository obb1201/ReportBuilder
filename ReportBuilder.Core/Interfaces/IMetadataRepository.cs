using ReportBuilder.Core;

namespace ReportBuilder.Core;

/// <summary>
/// Repository interface for metadata operations
/// </summary>
public interface IMetadataRepository
{
    /// <summary>
    /// Get all objects with basic information (no fields/relationships)
    /// </summary>
    Task<List<MetadataObject>> GetAllObjectsAsync(bool includeCustomOnly = false);

    /// <summary>
    /// Get a specific object by API name with all fields and relationships
    /// </summary>
    Task<MetadataObject?> GetObjectByNameAsync(string apiName);

    /// <summary>
    /// Get all fields for a specific object
    /// </summary>
    Task<List<MetadataField>> GetFieldsForObjectAsync(string objectApiName);

    /// <summary>
    /// Get all relationships for a specific object
    /// </summary>
    Task<List<MetadataRelationship>> GetRelationshipsForObjectAsync(string objectApiName);

    /// <summary>
    /// Get all child relationships for a specific object
    /// </summary>
    Task<List<ChildRelationship>> GetChildRelationshipsForObjectAsync(string objectApiName);

    /// <summary>
    /// Sync metadata from WSDL or Salesforce API
    /// </summary>
    Task SyncMetadataAsync(List<MetadataObject> objects);

    /// <summary>
    /// Clear all metadata (for fresh sync)
    /// </summary>
    Task ClearAllMetadataAsync();

    /// <summary>
    /// Get metadata sync status
    /// </summary>
    Task<MetadataSyncStatus?> GetSyncStatusAsync();

    /// <summary>
    /// Update sync status
    /// </summary>
    Task UpdateSyncStatusAsync(MetadataSyncStatus status);

    /// <summary>
    /// Search objects by name or label
    /// </summary>
    Task<List<MetadataObject>> SearchObjectsAsync(string searchTerm);

    /// <summary>
    /// Get field by object and field name
    /// </summary>
    Task<MetadataField?> GetFieldAsync(string objectApiName, string fieldApiName);
}

/// <summary>
/// Represents the status of metadata synchronization
/// </summary>
public class MetadataSyncStatus
{
    public DateTime? LastSyncStarted { get; set; }
    public DateTime? LastSyncCompleted { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int ObjectsProcessed { get; set; }
    public int FieldsProcessed { get; set; }
    public int RelationshipsProcessed { get; set; }
}
