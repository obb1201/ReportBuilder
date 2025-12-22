using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportBuilder.Core.Models.Metadata;
using System.Text;

namespace ReportBuilder.Infrastructure.Services;

/// <summary>
/// Generates realistic sample data for Salesforce objects
/// </summary>
public class DataGeneratorService
{
    private readonly string _connectionString;
    private readonly ILogger<DataGeneratorService> _logger;
    private readonly Random _random = new Random();

    public DataGeneratorService(IConfiguration configuration, ILogger<DataGeneratorService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    /// <summary>
    /// Generate sample data for an object
    /// </summary>
    public async Task<int> GenerateDataAsync(MetadataObject metadataObject, int recordCount = 500)
    {
        try
        {
            var tableName = SanitizeTableName(metadataObject.ApiName);
            
            _logger.LogInformation("Generating {Count} records for {Object}", recordCount, metadataObject.ApiName);
            
            var records = GenerateRecords(metadataObject, recordCount);
            var inserted = await InsertRecordsAsync(tableName, metadataObject.Fields.ToList(), records);
            
            _logger.LogInformation("Inserted {Count} records into {Table}", inserted, tableName);
            
            return inserted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data for {Object}", metadataObject.ApiName);
            return 0;
        }
    }

    /// <summary>
    /// Generate records using Bogus
    /// </summary>
    private List<Dictionary<string, object?>> GenerateRecords(MetadataObject metadataObject, int count)
    {
        var records = new List<Dictionary<string, object?>>();
        
        for (int i = 0; i < count; i++)
        {
            var record = new Dictionary<string, object?>();
            
            // Generate Id (Salesforce format: 18 characters)
            record["Id"] = GenerateSalesforceId();
            
            // Generate data for each field
            foreach (var field in metadataObject.Fields.Where(f => f.ApiName != "Id"))
            {
                record[field.ApiName] = GenerateFieldValue(field, metadataObject.ApiName, i);
            }
            
            records.Add(record);
        }
        
        return records;
    }

    /// <summary>
    /// Generate value for a specific field
    /// </summary>
    private object? GenerateFieldValue(MetadataField field, string objectName, int index)
    {
        var faker = new Faker();
        
        // Handle required fields
        if (field.IsRequired && _random.Next(100) < 5) // 5% chance of null for required fields (simulate bad data)
        {
            return null;
        }
        
        // Handle optional fields (30% null rate)
        if (!field.IsRequired && _random.Next(100) < 30)
        {
            return null;
        }
        
        // Generate based on field name patterns and type
        var fieldNameLower = field.ApiName.ToLower();
        
        // Common field patterns
        if (fieldNameLower == "name")
            return GenerateNameField(objectName, faker);
        
        if (fieldNameLower.Contains("email"))
            return faker.Internet.Email();
        
        if (fieldNameLower.Contains("phone"))
            return faker.Phone.PhoneNumber();
        
        if (fieldNameLower.Contains("website") || fieldNameLower.Contains("url"))
            return faker.Internet.Url();
        
        if (fieldNameLower.Contains("street") || fieldNameLower.Contains("address"))
            return faker.Address.StreetAddress();
        
        if (fieldNameLower.Contains("city"))
            return faker.Address.City();
        
        if (fieldNameLower.Contains("state"))
            return faker.Address.StateAbbr();
        
        if (fieldNameLower.Contains("postalcode") || fieldNameLower.Contains("zipcode"))
            return faker.Address.ZipCode();
        
        if (fieldNameLower.Contains("country"))
            return faker.Address.Country();
        
        if (fieldNameLower.Contains("description"))
            return faker.Lorem.Sentence(10);
        
        if (fieldNameLower.Contains("title"))
            return faker.Name.JobTitle();
        
        if (fieldNameLower.Contains("company"))
            return faker.Company.CompanyName();
        
        if (fieldNameLower.Contains("industry"))
            return faker.PickRandom(new[] { "Technology", "Healthcare", "Finance", "Manufacturing", "Retail", "Education", "Government" });
        
        // Generate by data type
        return field.DataType switch
        {
            FieldDataType.String => faker.Lorem.Word(),
            FieldDataType.Textarea => faker.Lorem.Paragraph(),
            FieldDataType.Email => faker.Internet.Email(),
            FieldDataType.Phone => faker.Phone.PhoneNumber(),
            FieldDataType.Url => faker.Internet.Url(),
            FieldDataType.Picklist => GeneratePicklistValue(field),
            FieldDataType.Int => faker.Random.Int(0, 1000000),
            FieldDataType.Double => Math.Round(faker.Random.Double(0, 1000000), 2),
            FieldDataType.Currency => Math.Round(faker.Random.Double(100, 10000000), 2),
            FieldDataType.Percent => Math.Round(faker.Random.Double(0, 100), 2),
            FieldDataType.Boolean => faker.Random.Bool(),
            FieldDataType.Date => faker.Date.Between(DateTime.Now.AddYears(-5), DateTime.Now.AddYears(2)),
            FieldDataType.DateTime => faker.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now),
            FieldDataType.Reference => GenerateSalesforceId(), // Reference to another record
            _ => faker.Lorem.Word()
        };
    }

    /// <summary>
    /// Generate Name field based on object type
    /// </summary>
    private string GenerateNameField(string objectName, Faker faker)
    {
        return objectName.ToLower() switch
        {
            "account" => faker.Company.CompanyName(),
            "contact" => faker.Name.FullName(),
            "lead" => faker.Name.FullName(),
            "opportunity" => $"{faker.Company.CompanyName()} - {faker.Commerce.ProductName()}",
            "case" => $"Case #{faker.Random.Number(100000, 999999)}",
            "campaign" => $"{faker.Commerce.ProductAdjective()} Campaign",
            "product2" => faker.Commerce.ProductName(),
            _ => $"{objectName} {faker.Random.Number(1000, 9999)}"
        };
    }

    /// <summary>
    /// Generate picklist value
    /// </summary>
    private string GeneratePicklistValue(MetadataField field)
    {
        var commonValues = new[] { "Active", "Inactive", "Pending", "Completed", "In Progress" };
        return new Faker().PickRandom(commonValues);
    }

    /// <summary>
    /// Generate Salesforce-style ID (18 characters)
    /// </summary>
    private string GenerateSalesforceId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, 18)
            .Select(_ => chars[_random.Next(chars.Length)])
            .ToArray());
    }

    /// <summary>
    /// Insert records into database
    /// </summary>
    private async Task<int> InsertRecordsAsync(string tableName, List<MetadataField> fields, List<Dictionary<string, object?>> records)
    {
        if (records.Count == 0) return 0;
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var insertedCount = 0;
        var batchSize = 100;
        
        for (int i = 0; i < records.Count; i += batchSize)
        {
            var batch = records.Skip(i).Take(batchSize).ToList();
            insertedCount += await InsertBatchAsync(connection, tableName, fields, batch);
        }
        
        return insertedCount;
    }

    /// <summary>
    /// Insert a batch of records
    /// </summary>
    private async Task<int> InsertBatchAsync(SqlConnection connection, string tableName, List<MetadataField> fields, List<Dictionary<string, object?>> batch)
    {
        var columnNames = new List<string> { "Id" };
        columnNames.AddRange(fields.Where(f => f.ApiName != "Id").Select(f => SanitizeColumnName(f.ApiName)));
        
        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO [{tableName}] ([{string.Join("], [", columnNames)}])");
        sql.AppendLine("VALUES");
        
        var parameters = new List<SqlParameter>();
        
        for (int i = 0; i < batch.Count; i++)
        {
            var record = batch[i];
            var valueParams = new List<string>();
            
            foreach (var col in columnNames)
            {
                var paramName = $"@p{i}_{col}";
                valueParams.Add(paramName);
                
                var value = record.ContainsKey(col) ? record[col] : null;
                parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
            }
            
            sql.Append($"    ({string.Join(", ", valueParams)})");
            
            if (i < batch.Count - 1)
                sql.AppendLine(",");
            else
                sql.AppendLine(";");
        }
        
        using var command = new SqlCommand(sql.ToString(), connection);
        command.Parameters.AddRange(parameters.ToArray());
        
        return await command.ExecuteNonQueryAsync();
    }

    private string SanitizeTableName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    private string SanitizeColumnName(string name)
    {
        return SanitizeTableName(name);
    }
}
