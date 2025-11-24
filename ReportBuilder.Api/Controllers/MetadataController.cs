using Microsoft.AspNetCore.Mvc;
using ReportBuilder.Core;

namespace ReportBuilder.Api.Controllers;

/// <summary>
/// API endpoints for metadata operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataRepository _repository;
    private readonly IWsdlMetadataExtractor _wsdlExtractor;
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(
        IMetadataRepository repository,
        IWsdlMetadataExtractor wsdlExtractor,
        ILogger<MetadataController> logger)
    {
        _repository = repository;
        _wsdlExtractor = wsdlExtractor;
        _logger = logger;
    }

    /// <summary>
    /// Get all objects with basic information
    /// </summary>
    /// <param name="customOnly">Return only custom objects</param>
    [HttpGet("objects")]
    [ProducesResponseType(typeof(List<MetadataObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetObjects([FromQuery] bool customOnly = false)
    {
        try
        {
            var objects = await _repository.GetAllObjectsAsync(customOnly);
            return Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving objects");
            return StatusCode(500, new { error = "Failed to retrieve objects", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific object by API name with all fields and relationships
    /// </summary>
    /// <param name="apiName">API name of the object (e.g., Account, Contact)</param>
    [HttpGet("objects/{apiName}")]
    [ProducesResponseType(typeof(MetadataObject), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetObject(string apiName)
    {
        try
        {
            var obj = await _repository.GetObjectByNameAsync(apiName);
            
            if (obj == null)
            {
                return NotFound(new { error = $"Object '{apiName}' not found" });
            }

            return Ok(obj);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving object {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to retrieve object", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all fields for a specific object
    /// </summary>
    /// <param name="apiName">API name of the object</param>
    [HttpGet("objects/{apiName}/fields")]
    [ProducesResponseType(typeof(List<MetadataField>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFields(string apiName)
    {
        try
        {
            var fields = await _repository.GetFieldsForObjectAsync(apiName);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fields for object {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to retrieve fields", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all relationships for a specific object
    /// </summary>
    /// <param name="apiName">API name of the object</param>
    [HttpGet("objects/{apiName}/relationships")]
    [ProducesResponseType(typeof(List<MetadataRelationship>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelationships(string apiName)
    {
        try
        {
            var relationships = await _repository.GetRelationshipsForObjectAsync(apiName);
            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationships for object {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to retrieve relationships", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all child relationships for a specific object
    /// </summary>
    /// <param name="apiName">API name of the object</param>
    [HttpGet("objects/{apiName}/child-relationships")]
    [ProducesResponseType(typeof(List<ChildRelationship>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildRelationships(string apiName)
    {
        try
        {
            var childRelationships = await _repository.GetChildRelationshipsForObjectAsync(apiName);
            return Ok(childRelationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child relationships for object {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to retrieve child relationships", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific field by object and field name
    /// </summary>
    /// <param name="objectApiName">API name of the object</param>
    /// <param name="fieldApiName">API name of the field</param>
    [HttpGet("objects/{objectApiName}/fields/{fieldApiName}")]
    [ProducesResponseType(typeof(MetadataField), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetField(string objectApiName, string fieldApiName)
    {
        try
        {
            var field = await _repository.GetFieldAsync(objectApiName, fieldApiName);
            
            if (field == null)
            {
                return NotFound(new { error = $"Field '{fieldApiName}' not found on object '{objectApiName}'" });
            }

            return Ok(field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving field {FieldApiName} for object {ObjectApiName}", fieldApiName, objectApiName);
            return StatusCode(500, new { error = "Failed to retrieve field", details = ex.Message });
        }
    }

    /// <summary>
    /// Search objects by name or label
    /// </summary>
    /// <param name="query">Search term</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<MetadataObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchObjects([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Query parameter is required" });
        }

        try
        {
            var objects = await _repository.SearchObjectsAsync(query);
            return Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching objects with query {Query}", query);
            return StatusCode(500, new { error = "Failed to search objects", details = ex.Message });
        }
    }

    /// <summary>
    /// Sync metadata from WSDL file
    /// </summary>
    [HttpPost("sync/from-wsdl")]
    [ProducesResponseType(typeof(SyncResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncFromWsdl([FromBody] WsdlSyncRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WsdlPath))
        {
            return BadRequest(new { error = "WsdlPath is required" });
        }

        try
        {
            _logger.LogInformation("Starting WSDL sync from: {WsdlPath}", request.WsdlPath);

            // Validate WSDL first
            var validation = await _wsdlExtractor.ValidateWsdlAsync(request.WsdlPath);
            if (!validation.IsValid)
            {
                return BadRequest(new 
                { 
                    error = "WSDL validation failed", 
                    errors = validation.Errors,
                    warnings = validation.Warnings 
                });
            }

            // Extract metadata
            var objects = await _wsdlExtractor.ExtractFromWsdlAsync(request.WsdlPath);

            // Clear existing if requested
            if (request.ClearExisting)
            {
                await _repository.ClearAllMetadataAsync();
            }

            // Sync to database
            await _repository.SyncMetadataAsync(objects);

            var result = new SyncResult
            {
                Success = true,
                ObjectsProcessed = objects.Count,
                FieldsProcessed = objects.Sum(o => o.Fields.Count),
                RelationshipsProcessed = objects.Sum(o => o.Relationships.Count + o.ChildRelationships.Count),
                Message = "Metadata synced successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing metadata from WSDL");
            return StatusCode(500, new { error = "Failed to sync metadata", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate a WSDL file without syncing
    /// </summary>
    [HttpPost("validate-wsdl")]
    [ProducesResponseType(typeof(WsdlValidationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateWsdl([FromBody] WsdlValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WsdlPath))
        {
            return BadRequest(new { error = "WsdlPath is required" });
        }

        try
        {
            var result = await _wsdlExtractor.ValidateWsdlAsync(request.WsdlPath);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating WSDL");
            return StatusCode(500, new { error = "Failed to validate WSDL", details = ex.Message });
        }
    }

    /// <summary>
    /// Get metadata sync status
    /// </summary>
    [HttpGet("sync/status")]
    [ProducesResponseType(typeof(MetadataSyncStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var status = await _repository.GetSyncStatusAsync();
            
            if (status == null)
            {
                return Ok(new { message = "No sync has been performed yet" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync status");
            return StatusCode(500, new { error = "Failed to retrieve sync status", details = ex.Message });
        }
    }

    /// <summary>
    /// Clear all metadata (use with caution)
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearMetadata()
    {
        try
        {
            await _repository.ClearAllMetadataAsync();
            return Ok(new { message = "All metadata cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing metadata");
            return StatusCode(500, new { error = "Failed to clear metadata", details = ex.Message });
        }
    }
}

// Request/Response DTOs

public class WsdlSyncRequest
{
    public string WsdlPath { get; set; } = string.Empty;
    public bool ClearExisting { get; set; } = false;
}

public class WsdlValidationRequest
{
    public string WsdlPath { get; set; } = string.Empty;
}

public class SyncResult
{
    public bool Success { get; set; }
    public int ObjectsProcessed { get; set; }
    public int FieldsProcessed { get; set; }
    public int RelationshipsProcessed { get; set; }
    public string Message { get; set; } = string.Empty;
}
