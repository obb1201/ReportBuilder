using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReportBuilder.Metadata.Services;

namespace ReportBuilder.WsdlTester;

/// <summary>
/// Console application to test WSDL metadata extraction
/// Usage: dotnet run -- path/to/enterprise.wsdl output.json
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Salesforce WSDL Metadata Extractor                       ║");
        Console.WriteLine("║  Report Builder - Metadata Service Layer                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run -- <wsdl-path> [output-json-path]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- enterprise.wsdl");
            Console.WriteLine("  dotnet run -- enterprise.wsdl metadata.json");
            Console.WriteLine("  dotnet run -- C:\\Salesforce\\enterprise.wsdl output.json");
            return;
        }

        string wsdlPath = args[0];
        string outputPath = args.Length > 1 ? args[1] : "metadata_output.json";

        // Set up logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<WsdlMetadataExtractor>();
        var extractor = new WsdlMetadataExtractor(logger);

        try
        {
            // Validate WSDL first
            Console.WriteLine($"📄 Validating WSDL: {wsdlPath}");
            var validation = await extractor.ValidateWsdlAsync(wsdlPath);

            if (!validation.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ WSDL validation failed!");
                Console.ResetColor();
                
                foreach (var error in validation.Errors)
                {
                    Console.WriteLine($"   ERROR: {error}");
                }
                
                foreach (var warning in validation.Warnings)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   WARNING: {warning}");
                    Console.ResetColor();
                }
                
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ WSDL validation passed");
            Console.ResetColor();
            Console.WriteLine($"  Objects found: {validation.ObjectCount}");
            if (!string.IsNullOrEmpty(validation.SalesforceVersion))
            {
                Console.WriteLine($"  Salesforce version: {validation.SalesforceVersion}");
            }
            Console.WriteLine();

            // Extract metadata
            Console.WriteLine("🔄 Extracting metadata from WSDL...");
            var startTime = DateTime.Now;
            
            var objects = await extractor.ExtractFromWsdlAsync(wsdlPath);
            
            var duration = DateTime.Now - startTime;
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Extraction completed in {duration.TotalSeconds:F2} seconds");
            Console.ResetColor();
            Console.WriteLine();

            // Display summary statistics
            DisplaySummaryStatistics(objects);

            // Display sample objects
            DisplaySampleObjects(objects);

            // Export to JSON
            Console.WriteLine($"💾 Exporting metadata to: {outputPath}");
            await ExportToJsonAsync(objects, outputPath);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Metadata exported successfully");
            Console.ResetColor();
            
            var fileInfo = new FileInfo(outputPath);
            Console.WriteLine($"  File size: {fileInfo.Length / 1024.0:F2} KB");
            Console.WriteLine();

            Console.WriteLine("✅ All operations completed successfully!");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Review the exported JSON file");
            Console.WriteLine("  2. Import this metadata into your SQL Server database");
            Console.WriteLine("  3. Use the API endpoints to access metadata programmatically");
        }
        catch (FileNotFoundException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ File not found: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    static void DisplaySummaryStatistics(List<ReportBuilder.Core.Models.Metadata.MetadataObject> objects)
    {
        Console.WriteLine("📊 Summary Statistics");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        
        var totalObjects = objects.Count;
        var standardObjects = objects.Count(o => o.IsStandard);
        var customObjects = objects.Count(o => o.IsCustom);
        var totalFields = objects.Sum(o => o.Fields.Count);
        var totalRelationships = objects.Sum(o => o.Relationships.Count);
        var totalChildRelationships = objects.Sum(o => o.ChildRelationships.Count);
        
        Console.WriteLine($"  Total Objects:           {totalObjects,6}");
        Console.WriteLine($"  Standard Objects:        {standardObjects,6}");
        Console.WriteLine($"  Custom Objects:          {customObjects,6}");
        Console.WriteLine($"  Total Fields:            {totalFields,6}");
        Console.WriteLine($"  Reference Relationships: {totalRelationships,6}");
        Console.WriteLine($"  Child Relationships:     {totalChildRelationships,6}");
        Console.WriteLine();

        // Field type breakdown
        var fieldsByType = objects
            .SelectMany(o => o.Fields)
            .GroupBy(f => f.DataType)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .ToList();

        Console.WriteLine("  Top 5 Field Types:");
        foreach (var group in fieldsByType)
        {
            Console.WriteLine($"    {group.Key,-15} {group.Count(),6}");
        }
        Console.WriteLine();
    }

    static void DisplaySampleObjects(List<ReportBuilder.Core.Models.Metadata.MetadataObject> objects)
    {
        Console.WriteLine("📋 Sample Objects (first 10)");
        Console.WriteLine("════════════════════════════════════════════════════════════");
        
        var samples = objects.Take(10).ToList();
        
        foreach (var obj in samples)
        {
            var objectType = obj.IsCustom ? "[Custom]" : "[Standard]";
            Console.WriteLine($"  {obj.ApiName,-30} {objectType,-12} {obj.Fields.Count,3} fields");
        }
        
        if (objects.Count > 10)
        {
            Console.WriteLine($"  ... and {objects.Count - 10} more objects");
        }
        Console.WriteLine();
    }

    static async Task ExportToJsonAsync(
        List<ReportBuilder.Core.Models.Metadata.MetadataObject> objects, 
        string outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(objects, options);
        await File.WriteAllTextAsync(outputPath, json);
    }
}
