using System.Net.Http.Json;
using ReportBuilder.Core.Models.Metadata;

namespace ReportBuilder.Web.Services;

/// <summary>
/// Service for calling the Metadata API
/// </summary>
public class MetadataApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetadataApiClient> _logger;

    public MetadataApiClient(HttpClient httpClient, ILogger<MetadataApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all metadata objects
    /// </summary>
    public async Task<List<MetadataObject>> GetObjectsAsync(bool customOnly = false)
    {
        try
        {
            var url = customOnly ? "/api/metadata/objects?customOnly=true" : "/api/metadata/objects";
            var response = await _httpClient.GetFromJsonAsync<List<MetadataObject>>(url);
            return response ?? new List<MetadataObject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting objects from API");
            return new List<MetadataObject>();
        }
    }

    /// <summary>
    /// Get a specific object by API name
    /// </summary>
    public async Task<MetadataObject?> GetObjectAsync(string apiName)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MetadataObject>($"/api/metadata/objects/{apiName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting object {ApiName} from API", apiName);
            return null;
        }
    }

    /// <summary>
    /// Get fields for a specific object
    /// </summary>
    public async Task<List<MetadataField>> GetFieldsAsync(string objectApiName)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<MetadataField>>(
                $"/api/metadata/objects/{objectApiName}/fields");
            return response ?? new List<MetadataField>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fields for {ObjectApiName} from API", objectApiName);
            return new List<MetadataField>();
        }
    }

    /// <summary>
    /// Get relationships for a specific object
    /// </summary>
    public async Task<List<MetadataRelationship>> GetRelationshipsAsync(string objectApiName)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<MetadataRelationship>>(
                $"/api/metadata/objects/{objectApiName}/relationships");
            return response ?? new List<MetadataRelationship>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationships for {ObjectApiName} from API", objectApiName);
            return new List<MetadataRelationship>();
        }
    }

    /// <summary>
    /// Search objects by name or label
    /// </summary>
    public async Task<List<MetadataObject>> SearchObjectsAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<MetadataObject>>(
                $"/api/metadata/search?query={Uri.EscapeDataString(searchTerm)}");
            return response ?? new List<MetadataObject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching objects with term {SearchTerm}", searchTerm);
            return new List<MetadataObject>();
        }
    }
}
