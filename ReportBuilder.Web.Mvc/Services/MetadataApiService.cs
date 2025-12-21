using System.Text.Json;
using ReportBuilder.Core.Models.Metadata;

namespace ReportBuilder.Web.Mvc.Services;

/// <summary>
/// Service for calling the Metadata API
/// </summary>
public class MetadataApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MetadataApiService> _logger;

    public MetadataApiService(IHttpClientFactory httpClientFactory, ILogger<MetadataApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all metadata objects
    /// </summary>
    public async Task<List<MetadataObject>> GetObjectsAsync(bool customOnly = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MetadataApi");
            var url = customOnly ? "/api/metadata/objects?customOnly=true" : "/api/metadata/objects";
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var objects = JsonSerializer.Deserialize<List<MetadataObject>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return objects ?? new List<MetadataObject>();
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
            var client = _httpClientFactory.CreateClient("MetadataApi");
            var response = await client.GetAsync($"/api/metadata/objects/{apiName}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<MetadataObject>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return obj;
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
            var client = _httpClientFactory.CreateClient("MetadataApi");
            var response = await client.GetAsync($"/api/metadata/objects/{objectApiName}/fields");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var fields = JsonSerializer.Deserialize<List<MetadataField>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return fields ?? new List<MetadataField>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fields for {ObjectApiName} from API", objectApiName);
            return new List<MetadataField>();
        }
    }

    /// <summary>
    /// Search objects by name or label
    /// </summary>
    public async Task<List<MetadataObject>> SearchObjectsAsync(string searchTerm)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MetadataApi");
            var response = await client.GetAsync($"/api/metadata/search?query={Uri.EscapeDataString(searchTerm)}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var objects = JsonSerializer.Deserialize<List<MetadataObject>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return objects ?? new List<MetadataObject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching objects with term {SearchTerm}", searchTerm);
            return new List<MetadataObject>();
        }
    }
}
