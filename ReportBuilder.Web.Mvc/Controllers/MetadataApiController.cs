using Microsoft.AspNetCore.Mvc;
using ReportBuilder.Web.Mvc.Services;

namespace ReportBuilder.Web.Mvc.Controllers;

/// <summary>
/// API Controller for metadata operations - proxies calls to the backend API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetadataApiController : ControllerBase
{
    private readonly MetadataApiService _metadataService;
    private readonly ILogger<MetadataApiController> _logger;

    public MetadataApiController(
        MetadataApiService metadataService,
        ILogger<MetadataApiController> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/MetadataApi/objects
    /// Get all Salesforce objects
    /// </summary>
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjects([FromQuery] bool customOnly = false)
    {
        try
        {
            var objects = await _metadataService.GetObjectsAsync(customOnly);
            return Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting objects");
            return StatusCode(500, new { error = "Failed to load objects" });
        }
    }

    /// <summary>
    /// GET: api/MetadataApi/objects/{apiName}
    /// Get a specific object with all its fields
    /// </summary>
    [HttpGet("objects/{apiName}")]
    public async Task<IActionResult> GetObject(string apiName)
    {
        try
        {
            var obj = await _metadataService.GetObjectAsync(apiName);
            
            if (obj == null)
            {
                return NotFound(new { error = $"Object '{apiName}' not found" });
            }
            
            return Ok(obj);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting object {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to load object" });
        }
    }

    /// <summary>
    /// GET: api/MetadataApi/objects/{apiName}/fields
    /// Get fields for a specific object
    /// </summary>
    [HttpGet("objects/{apiName}/fields")]
    public async Task<IActionResult> GetFields(string apiName)
    {
        try
        {
            var fields = await _metadataService.GetFieldsAsync(apiName);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fields for {ApiName}", apiName);
            return StatusCode(500, new { error = "Failed to load fields" });
        }
    }

    /// <summary>
    /// GET: api/MetadataApi/search?query={searchTerm}
    /// Search objects by name or label
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchObjects([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            var objects = await _metadataService.SearchObjectsAsync(query);
            return Ok(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching objects with query {Query}", query);
            return StatusCode(500, new { error = "Failed to search objects" });
        }
    }
}
