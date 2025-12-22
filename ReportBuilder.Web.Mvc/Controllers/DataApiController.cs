using Microsoft.AspNetCore.Mvc;
using ReportBuilder.Web.Mvc.Services;

namespace ReportBuilder.Web.Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataApiController : ControllerBase
{
    private readonly DataApiService _dataApiService;
    private readonly ILogger<DataApiController> _logger;

    public DataApiController(DataApiService dataApiService, ILogger<DataApiController> logger)
    {
        _dataApiService = dataApiService;
        _logger = logger;
    }

    [HttpPost("setup/{objectName}")]
    public async Task<IActionResult> SetupObject(string objectName, [FromQuery] int recordCount = 500)
    {
        var result = await _dataApiService.SetupObjectAsync(objectName, recordCount);
        
        if (result == null)
        {
            return StatusCode(500, new { error = "Failed to setup object" });
        }

        return Ok(result);
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        var result = await _dataApiService.ExecuteQueryAsync(request.SoqlQuery);
        
        if (result == null)
        {
            return StatusCode(500, new { error = "Failed to execute query" });
        }

        return Ok(result);
    }

    [HttpGet("check/{objectName}")]
    public async Task<IActionResult> CheckObjectData(string objectName)
    {
        var hasData = await _dataApiService.CheckObjectHasDataAsync(objectName);
        return Ok(new { objectName, hasData });
    }

    [HttpGet("objects")]
    public async Task<IActionResult> GetPopulatedObjects()
    {
        var objects = await _dataApiService.GetPopulatedObjectsAsync();
        return Ok(objects ?? new List<PopulatedObject>());
    }
}

public class ExecuteQueryRequest
{
    public string SoqlQuery { get; set; } = string.Empty;
}
