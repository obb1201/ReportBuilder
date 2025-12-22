using System.Text;
using System.Text.Json;

namespace ReportBuilder.Web.Mvc.Services;

public class DataApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataApiService> _logger;

    public DataApiService(IHttpClientFactory httpClientFactory, ILogger<DataApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DataApi");
        _logger = logger;
    }

    /// <summary>
    /// Setup an object with sample data
    /// </summary>
    public async Task<SetupObjectResponse?> SetupObjectAsync(string objectName, int recordCount = 500)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"/api/data/setup/{objectName}?recordCount={recordCount}",
                null);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SetupObjectResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up object {ObjectName}", objectName);
            return null;
        }
    }

    /// <summary>
    /// Execute SOQL query
    /// </summary>
    public async Task<QueryExecutionResponse?> ExecuteQueryAsync(string soqlQuery)
    {
        try
        {
            var request = new { soqlQuery = soqlQuery };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/data/execute", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Parse error response
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new QueryExecutionResponse
                {
                    Success = false,
                    Error = errorResponse?.Error ?? "Unknown error occurred"
                };
            }

            return JsonSerializer.Deserialize<QueryExecutionResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query");
            return new QueryExecutionResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Get list of objects with data
    /// </summary>
    public async Task<List<PopulatedObject>?> GetPopulatedObjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/data/objects");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PopulatedObject>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting populated objects");
            return null;
        }
    }

    /// <summary>
    /// Check if object has data
    /// </summary>
    public async Task<bool> CheckObjectHasDataAsync(string objectName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/data/check/{objectName}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckDataResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.HasData ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking object data");
            return false;
        }
    }
}

// Response models
public class SetupObjectResponse
{
    public bool Success { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public int RecordsGenerated { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class QueryExecutionResponse
{
    public bool Success { get; set; }
    public List<Dictionary<string, object?>>? Data { get; set; }
    public List<ColumnInfo>? Columns { get; set; }
    public int RecordCount { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? SqlQuery { get; set; }
    public string? Error { get; set; }
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

public class PopulatedObject
{
    public string ApiName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool HasData { get; set; }
}

public class CheckDataResponse
{
    public string ObjectName { get; set; } = string.Empty;
    public bool HasData { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}
