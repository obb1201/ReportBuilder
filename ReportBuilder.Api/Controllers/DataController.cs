using Microsoft.AspNetCore.Mvc;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Infrastructure.Services;
using System.Data;

namespace ReportBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly DynamicTableService _tableService;
    private readonly DataGeneratorService _dataGenerator;
    private readonly QueryExecutionService _queryExecutor;
    private readonly ILogger<DataController> _logger;

    public DataController(
        IMetadataRepository metadataRepository,
        DynamicTableService tableService,
        DataGeneratorService dataGenerator,
        QueryExecutionService queryExecutor,
        ILogger<DataController> logger)
    {
        _metadataRepository = metadataRepository;
        _tableService = tableService;
        _dataGenerator = dataGenerator;
        _queryExecutor = queryExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Setup an object: Create table and generate sample data
    /// </summary>
    [HttpPost("setup/{objectName}")]
    public async Task<IActionResult> SetupObject(string objectName, [FromQuery] int recordCount = 500)
    {
        try
        {
            _logger.LogInformation("Setting up object: {ObjectName} with {RecordCount} records", objectName, recordCount);

            // Get metadata for object with fields
            var metadataObject = await _metadataRepository.GetObjectByNameAsync(objectName);
            if (metadataObject == null)
            {
                return NotFound(new { error = $"Object '{objectName}' not found in metadata" });
            }

            // Get fields for the object
            var fields = await _metadataRepository.GetFieldsForObjectAsync(objectName);
            metadataObject.Fields = fields;

            // Create table
            var tableCreated = await _tableService.CreateTableForObjectAsync(metadataObject);
            if (!tableCreated)
            {
                return BadRequest(new { error = $"Failed to create table for {objectName}" });
            }

            // Generate data
            var recordsGenerated = await _dataGenerator.GenerateDataAsync(metadataObject, recordCount);

            return Ok(new
            {
                success = true,
                objectName = objectName,
                recordsGenerated = recordsGenerated,
                message = $"Successfully created {objectName} table with {recordsGenerated} records"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up object {ObjectName}", objectName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute a SOQL query
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        try
        {
            _logger.LogInformation("Executing SOQL query: {Query}", request.SoqlQuery);

            var result = await _queryExecutor.ExecuteSoqlQueryAsync(request.SoqlQuery);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage
                });
            }

            // Convert DataTable to list of dictionaries for JSON serialization
            var rows = new List<Dictionary<string, object?>>();
            if (result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (DataColumn col in result.Data.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    rows.Add(dict);
                }
            }

            // Get column metadata
            var columns = new List<object>();
            if (result.Data != null)
            {
                foreach (DataColumn col in result.Data.Columns)
                {
                    columns.Add(new
                    {
                        name = col.ColumnName,
                        dataType = col.DataType.Name
                    });
                }
            }

            return Ok(new
            {
                success = true,
                data = rows,
                columns = columns,
                recordCount = result.RecordCount,
                executionTimeMs = result.ExecutionTimeMs,
                sqlQuery = result.SqlQuery
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get list of objects that have been populated with data
    /// </summary>
    [HttpGet("objects")]
    public async Task<IActionResult> GetPopulatedObjects()
    {
        try
        {
            var tables = await _tableService.GetCreatedTablesAsync();

            var objectsWithData = new List<object>();
            foreach (var table in tables)
            {
                var metadata = await _metadataRepository.GetObjectByNameAsync(table);
                objectsWithData.Add(new
                {
                    apiName = table,
                    label = metadata?.Label ?? table,
                    hasData = true
                });
            }

            return Ok(objectsWithData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting populated objects");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get query execution history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetQueryHistory([FromQuery] int top = 20)
    {
        try
        {
            var history = await _queryExecutor.GetQueryHistoryAsync(top);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query history");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if an object has data
    /// </summary>
    [HttpGet("check/{objectName}")]
    public async Task<IActionResult> CheckObjectData(string objectName)
    {
        try
        {
            var tables = await _tableService.GetCreatedTablesAsync();
            var hasData = tables.Contains(objectName, StringComparer.OrdinalIgnoreCase);

            return Ok(new
            {
                objectName = objectName,
                hasData = hasData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking object data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Regenerate data for an object
    /// </summary>
    [HttpPost("regenerate/{objectName}")]
    public async Task<IActionResult> RegenerateData(string objectName, [FromQuery] int recordCount = 500)
    {
        try
        {
            _logger.LogInformation("Regenerating data for {ObjectName}", objectName);

            // Drop existing table
            await _tableService.DropTableAsync(objectName);

            // Setup again
            return await SetupObject(objectName, recordCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating data for {ObjectName}", objectName);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class ExecuteQueryRequest
{
    public string SoqlQuery { get; set; } = string.Empty;
}
