using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Core.Models.Metadata;
using ReportBuilder.Infrastructure.Data;
using ReportBuilder.Infrastructure.Data.Entities;

namespace ReportBuilder.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for metadata operations using Entity Framework
/// </summary>
public class MetadataRepository : IMetadataRepository
{
    private readonly MetadataDbContext _context;
    private readonly ILogger<MetadataRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MetadataRepository(
        MetadataDbContext context,
        ILogger<MetadataRepository> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<List<MetadataObject>> GetAllObjectsAsync(bool includeCustomOnly = false)
    {
        var query = _context.MetadataObjects.AsQueryable();

        if (includeCustomOnly)
        {
            query = query.Where(o => o.IsCustom);
        }

        var entities = await query
            .OrderBy(o => o.Label)
            .ToListAsync();

        return entities.Select(MapEntityToModel).ToList();
    }

    public async Task<MetadataObject?> GetObjectByNameAsync(string apiName)
    {
        var entity = await _context.MetadataObjects
            .Include(o => o.Fields)
            .Include(o => o.Relationships)
            .Include(o => o.ChildRelationships)
            .FirstOrDefaultAsync(o => o.ApiName == apiName);

        if (entity == null)
            return null;

        return MapEntityToModelComplete(entity);
    }

    public async Task<List<MetadataField>> GetFieldsForObjectAsync(string objectApiName)
    {
        var objectEntity = await _context.MetadataObjects
            .Include(o => o.Fields)
            .FirstOrDefaultAsync(o => o.ApiName == objectApiName);

        if (objectEntity == null)
            return new List<MetadataField>();

        return objectEntity.Fields
            .Select(MapFieldEntityToModel)
            .OrderBy(f => f.Label)
            .ToList();
    }

    public async Task<List<MetadataRelationship>> GetRelationshipsForObjectAsync(string objectApiName)
    {
        var objectEntity = await _context.MetadataObjects
            .Include(o => o.Relationships)
            .FirstOrDefaultAsync(o => o.ApiName == objectApiName);

        if (objectEntity == null)
            return new List<MetadataRelationship>();

        return objectEntity.Relationships
            .Select(MapRelationshipEntityToModel)
            .ToList();
    }

    public async Task<List<ChildRelationship>> GetChildRelationshipsForObjectAsync(string objectApiName)
    {
        var objectEntity = await _context.MetadataObjects
            .Include(o => o.ChildRelationships)
            .FirstOrDefaultAsync(o => o.ApiName == objectApiName);

        if (objectEntity == null)
            return new List<ChildRelationship>();

        return objectEntity.ChildRelationships
            .Select(MapChildRelationshipEntityToModel)
            .ToList();
    }

    public async Task SyncMetadataAsync(List<MetadataObject> objects)
    {
        _logger.LogInformation("Starting metadata sync for {Count} objects", objects.Count);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var syncStatus = new MetadataSyncStatusEntity
            {
                LastSyncStarted = DateTime.UtcNow,
                IsSuccess = false
            };

            foreach (var obj in objects)
            {
                await UpsertObjectAsync(obj);
                syncStatus.ObjectsProcessed++;
                syncStatus.FieldsProcessed += obj.Fields.Count;
                syncStatus.RelationshipsProcessed += obj.Relationships.Count + obj.ChildRelationships.Count;
            }

            await _context.SaveChangesAsync();

            syncStatus.LastSyncCompleted = DateTime.UtcNow;
            syncStatus.IsSuccess = true;

            _context.MetadataSyncStatus.Add(syncStatus);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Metadata sync completed successfully. Objects: {Objects}, Fields: {Fields}, Relationships: {Rels}",
                syncStatus.ObjectsProcessed, syncStatus.FieldsProcessed, syncStatus.RelationshipsProcessed);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during metadata sync");
            throw;
        }
    }

    public async Task ClearAllMetadataAsync()
    {
        _logger.LogWarning("Clearing all metadata");

        // EF Core will handle cascading deletes
        await _context.MetadataObjects.ExecuteDeleteAsync();
        await _context.SaveChangesAsync();

        _logger.LogInformation("All metadata cleared");
    }

    public async Task<MetadataSyncStatus?> GetSyncStatusAsync()
    {
        var entity = await _context.MetadataSyncStatus
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new MetadataSyncStatus
        {
            LastSyncStarted = entity.LastSyncStarted,
            LastSyncCompleted = entity.LastSyncCompleted,
            IsSuccess = entity.IsSuccess,
            ErrorMessage = entity.ErrorMessage,
            ObjectsProcessed = entity.ObjectsProcessed,
            FieldsProcessed = entity.FieldsProcessed,
            RelationshipsProcessed = entity.RelationshipsProcessed
        };
    }

    public async Task UpdateSyncStatusAsync(MetadataSyncStatus status)
    {
        var entity = new MetadataSyncStatusEntity
        {
            LastSyncStarted = status.LastSyncStarted,
            LastSyncCompleted = status.LastSyncCompleted,
            IsSuccess = status.IsSuccess,
            ErrorMessage = status.ErrorMessage,
            ObjectsProcessed = status.ObjectsProcessed,
            FieldsProcessed = status.FieldsProcessed,
            RelationshipsProcessed = status.RelationshipsProcessed
        };

        _context.MetadataSyncStatus.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MetadataObject>> SearchObjectsAsync(string searchTerm)
    {
        var query = _context.MetadataObjects
            .Where(o => o.ApiName.Contains(searchTerm) || o.Label.Contains(searchTerm))
            .OrderBy(o => o.Label)
            .Take(50);

        var entities = await query.ToListAsync();
        return entities.Select(MapEntityToModel).ToList();
    }

    public async Task<MetadataField?> GetFieldAsync(string objectApiName, string fieldApiName)
    {
        var entity = await _context.MetadataFields
            .Include(f => f.Object)
            .FirstOrDefaultAsync(f => f.Object.ApiName == objectApiName && f.ApiName == fieldApiName);

        return entity != null ? MapFieldEntityToModel(entity) : null;
    }

    // Private helper methods

    private async Task UpsertObjectAsync(MetadataObject obj)
    {
        var existing = await _context.MetadataObjects
            .Include(o => o.Fields)
            .Include(o => o.Relationships)
            .Include(o => o.ChildRelationships)
            .FirstOrDefaultAsync(o => o.ApiName == obj.ApiName);

        if (existing != null)
        {
            // Update existing
            UpdateObjectEntity(existing, obj);
            _context.MetadataObjects.Update(existing);
        }
        else
        {
            // Create new
            var newEntity = MapModelToEntity(obj);
            _context.MetadataObjects.Add(newEntity);
        }
    }

    private void UpdateObjectEntity(MetadataObjectEntity entity, MetadataObject model)
    {
        entity.Label = model.Label;
        entity.Namespace = model.Namespace;
        entity.IsCustom = model.IsCustom;
        entity.IsStandard = model.IsStandard;
        entity.BaseType = model.BaseType;
        entity.CapabilitiesJson = JsonSerializer.Serialize(model.Capabilities, _jsonOptions);
        entity.AuditJson = JsonSerializer.Serialize(model.Audit, _jsonOptions);
        entity.LastSyncedAt = model.LastSyncedAt;
        entity.UpdatedAt = DateTime.UtcNow;

        // Clear and rebuild child collections
        entity.Fields.Clear();
        entity.Relationships.Clear();
        entity.ChildRelationships.Clear();

        foreach (var field in model.Fields)
        {
            entity.Fields.Add(MapFieldModelToEntity(field));
        }

        foreach (var rel in model.Relationships)
        {
            entity.Relationships.Add(MapRelationshipModelToEntity(rel));
        }

        foreach (var childRel in model.ChildRelationships)
        {
            entity.ChildRelationships.Add(MapChildRelationshipModelToEntity(childRel));
        }
    }

    private MetadataObjectEntity MapModelToEntity(MetadataObject model)
    {
        var entity = new MetadataObjectEntity
        {
            ApiName = model.ApiName,
            Label = model.Label,
            Namespace = model.Namespace,
            IsCustom = model.IsCustom,
            IsStandard = model.IsStandard,
            BaseType = model.BaseType,
            CapabilitiesJson = JsonSerializer.Serialize(model.Capabilities, _jsonOptions),
            AuditJson = JsonSerializer.Serialize(model.Audit, _jsonOptions),
            LastSyncedAt = model.LastSyncedAt
        };

        foreach (var field in model.Fields)
        {
            entity.Fields.Add(MapFieldModelToEntity(field));
        }

        foreach (var rel in model.Relationships)
        {
            entity.Relationships.Add(MapRelationshipModelToEntity(rel));
        }

        foreach (var childRel in model.ChildRelationships)
        {
            entity.ChildRelationships.Add(MapChildRelationshipModelToEntity(childRel));
        }

        return entity;
    }

    private MetadataObject MapEntityToModel(MetadataObjectEntity entity)
    {
        return new MetadataObject
        {
            ApiName = entity.ApiName,
            Label = entity.Label,
            Namespace = entity.Namespace,
            IsCustom = entity.IsCustom,
            IsStandard = entity.IsStandard,
            BaseType = entity.BaseType,
            Capabilities = JsonSerializer.Deserialize<ObjectCapabilities>(entity.CapabilitiesJson, _jsonOptions) ?? new ObjectCapabilities(),
            Audit = JsonSerializer.Deserialize<AuditInfo>(entity.AuditJson, _jsonOptions) ?? new AuditInfo(),
            LastSyncedAt = entity.LastSyncedAt
        };
    }

    private MetadataObject MapEntityToModelComplete(MetadataObjectEntity entity)
    {
        var model = MapEntityToModel(entity);
        
        model.Fields = entity.Fields.Select(MapFieldEntityToModel).ToList();
        model.Relationships = entity.Relationships.Select(MapRelationshipEntityToModel).ToList();
        model.ChildRelationships = entity.ChildRelationships.Select(MapChildRelationshipEntityToModel).ToList();

        return model;
    }

    private MetadataFieldEntity MapFieldModelToEntity(MetadataField model)
    {
        return new MetadataFieldEntity
        {
            ApiName = model.ApiName,
            Label = model.Label,
            DataType = model.DataType.ToString(),
            Length = model.Length,
            Precision = model.Precision,
            Scale = model.Scale,
            IsNillable = model.IsNillable,
            DefaultValue = model.DefaultValue,
            IsCalculated = model.IsCalculated,
            IsCustom = model.IsCustom,
            IsRequired = model.IsRequired,
            IsUnique = model.IsUnique,
            IsExternalId = model.IsExternalId,
            ReferenceTo = model.ReferenceTo,
            RelationshipName = model.RelationshipName,
            HelpText = model.HelpText,
            CapabilitiesJson = JsonSerializer.Serialize(model.Capabilities, _jsonOptions),
            PicklistValuesJson = model.PicklistValues != null 
                ? JsonSerializer.Serialize(model.PicklistValues, _jsonOptions) 
                : null
        };
    }

    private MetadataField MapFieldEntityToModel(MetadataFieldEntity entity)
    {
        return new MetadataField
        {
            ApiName = entity.ApiName,
            Label = entity.Label,
            DataType = Enum.Parse<FieldDataType>(entity.DataType),
            Length = entity.Length,
            Precision = entity.Precision,
            Scale = entity.Scale,
            IsNillable = entity.IsNillable,
            DefaultValue = entity.DefaultValue,
            IsCalculated = entity.IsCalculated,
            IsCustom = entity.IsCustom,
            IsRequired = entity.IsRequired,
            IsUnique = entity.IsUnique,
            IsExternalId = entity.IsExternalId,
            ReferenceTo = entity.ReferenceTo,
            RelationshipName = entity.RelationshipName,
            HelpText = entity.HelpText,
            Capabilities = JsonSerializer.Deserialize<FieldCapabilities>(entity.CapabilitiesJson, _jsonOptions) ?? new FieldCapabilities(),
            PicklistValues = !string.IsNullOrEmpty(entity.PicklistValuesJson)
                ? JsonSerializer.Deserialize<List<PicklistValue>>(entity.PicklistValuesJson, _jsonOptions)
                : null
        };
    }

    private MetadataRelationshipEntity MapRelationshipModelToEntity(MetadataRelationship model)
    {
        return new MetadataRelationshipEntity
        {
            FromField = model.FromField,
            ToObject = model.ToObject,
            RelationshipName = model.RelationshipName,
            Cardinality = model.Cardinality.ToString(),
            RelationshipType = model.RelationshipType.ToString(),
            IsRequired = model.IsRequired,
            IsPolymorphic = model.IsPolymorphic,
            PolymorphicTargetsJson = model.PolymorphicTargets != null
                ? JsonSerializer.Serialize(model.PolymorphicTargets, _jsonOptions)
                : null
        };
    }

    private MetadataRelationship MapRelationshipEntityToModel(MetadataRelationshipEntity entity)
    {
        return new MetadataRelationship
        {
            FromField = entity.FromField,
            ToObject = entity.ToObject,
            RelationshipName = entity.RelationshipName,
            Cardinality = Enum.Parse<RelationshipCardinality>(entity.Cardinality),
            RelationshipType = Enum.Parse<RelationshipType>(entity.RelationshipType),
            IsRequired = entity.IsRequired,
            IsPolymorphic = entity.IsPolymorphic,
            PolymorphicTargets = !string.IsNullOrEmpty(entity.PolymorphicTargetsJson)
                ? JsonSerializer.Deserialize<List<string>>(entity.PolymorphicTargetsJson, _jsonOptions)
                : null
        };
    }

    private ChildRelationshipEntity MapChildRelationshipModelToEntity(ChildRelationship model)
    {
        return new ChildRelationshipEntity
        {
            ChildObject = model.ChildObject,
            ViaForeignKey = model.ViaForeignKey,
            RelationshipName = model.RelationshipName,
            Cardinality = model.Cardinality.ToString(),
            IsCascadeDelete = model.IsCascadeDelete
        };
    }

    private ChildRelationship MapChildRelationshipEntityToModel(ChildRelationshipEntity entity)
    {
        return new ChildRelationship
        {
            ChildObject = entity.ChildObject,
            ViaForeignKey = entity.ViaForeignKey,
            RelationshipName = entity.RelationshipName,
            Cardinality = Enum.Parse<RelationshipCardinality>(entity.Cardinality),
            IsCascadeDelete = entity.IsCascadeDelete
        };
    }
}
