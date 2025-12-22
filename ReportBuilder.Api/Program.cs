using Microsoft.EntityFrameworkCore;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Infrastructure.Data;
using ReportBuilder.Infrastructure.Repositories;
using ReportBuilder.Infrastructure.Services;
using ReportBuilder.Metadata.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Report Builder Metadata API", 
        Version = "v1",
        Description = "API for managing Salesforce metadata for report builder"
    });
});

// Add database context
builder.Services.AddDbContext<MetadataDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MetadataDb"),
        sqlOptions => 
        {
            // Comment out the retry strategy to allow manual transactions
            // sqlOptions.EnableRetryOnFailure();
        });
});

// Register repositories and services
builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();
builder.Services.AddScoped<IWsdlMetadataExtractor, WsdlMetadataExtractor>();

// Register data services
builder.Services.AddScoped<DynamicTableService>();
builder.Services.AddScoped<DataGeneratorService>();
builder.Services.AddScoped<SoqlToSqlConverter>();
builder.Services.AddScoped<QueryExecutionService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Report Builder Metadata API v1");
    c.RoutePrefix = "swagger"; // Swagger at /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Report Builder Metadata API started");
logger.LogInformation("Swagger UI available at: https://localhost:{Port}", 
    app.Configuration["Kestrel:Endpoints:Https:Url"] ?? "5001");

app.Run();
