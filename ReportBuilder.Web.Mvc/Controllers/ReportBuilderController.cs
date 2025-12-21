using Microsoft.AspNetCore.Mvc;

namespace ReportBuilder.Web.Mvc.Controllers;

public class ReportBuilderController : Controller
{
    private readonly ILogger<ReportBuilderController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ReportBuilderController(
        ILogger<ReportBuilderController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult MetadataBrowser()
    {
        return View();
    }
}
