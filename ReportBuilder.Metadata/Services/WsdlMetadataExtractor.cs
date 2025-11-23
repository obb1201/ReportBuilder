using System.Xml;
using System.Xml.Linq;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Core.Models.Metadata;

namespace ReportBuilder.Metadata.Services;

/// <summary>
/// Extracts metadata from Salesforce Enterprise WSDL files
/// </summary>
public class WsdlMetadataExtractor : IWsdlMetadataExtractor
{
    private readonly ILogger<WsdlMetadataExtractor> _logger;
    private const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";
    private const string TnsNamespace = "urn:enterprise.soap.sforce.com";

    public WsdlMetadataExtractor(ILogger<WsdlMetadataExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<List<MetadataObject>> ExtractFromWsdlAsync(string wsdlPath)
    {
        _logger.LogInformation("Starting WSDL extraction from: {WsdlPath}", wsdlPath);

        if (!File.Exists(wsdlPath))
        {
            throw new FileNotFoundException($"WSDL file not found: {wsdlPath}");
        }

        using var stream = File.OpenRead(wsdlPath);
        return await ExtractFromWsdlStreamAsync(stream);
    }

    public async Task<List<MetadataObject>> ExtractFromWsdlStreamAsync(Stream wsdlStream)
    {
        var objects = new List<MetadataObject>();

        try
        {
            var doc = await XDocument.LoadAsync(wsdlStream, LoadOptions.None, CancellationToken.None);
            var ns = XNamespace.Get(XsdNamespace);

            // Find the schema element
            var schema = doc.Descendants(ns + "schema").FirstOrDefault();
            if (schema == null)
            {
                throw new InvalidOperationException("No schema element found in WSDL");
            }

            // Extract all complexType elements that represent sObjects
            var complexTypes = schema.Elements(ns + "complexType").ToList();
            _logger.LogInformation("Found {Count} complex types in WSDL", complexTypes.Count);

            foreach (var complexType in complexTypes)
            {
                var typeName = complexType.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(typeName))
                    continue;

                // Skip non-sObject types (utilities, results, etc.)
                if (IsUtilityType(typeName))
                    continue;

                var metadataObject = ExtractObjectFromComplexType(complexType, ns);
                if (metadataObject != null)
                {
                    objects.Add(metadataObject);
                }
            }

            _logger.LogInformation("Successfully extracted {Count} objects from WSDL", objects.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting metadata from WSDL");
            throw;
        }

        return objects;
    }

    public async Task<MetadataObject?> ExtractObjectFromWsdlAsync(string wsdlPath, string objectApiName)
    {
        var allObjects = await ExtractFromWsdlAsync(wsdlPath);
        return allObjects.FirstOrDefault(o => o.ApiName.Equals(objectApiName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<WsdlValidationResult> ValidateWsdlAsync(string wsdlPath)
    {
        var result = new WsdlValidationResult();

        try
        {
            if (!File.Exists(wsdlPath))
            {
                result.Errors.Add($"WSDL file not found: {wsdlPath}");
                return result;
            }

            var doc = await XDocument.LoadAsync(File.OpenRead(wsdlPath), LoadOptions.None, CancellationToken.None);
            var ns = XNamespace.Get(XsdNamespace);

            var schema = doc.Descendants(ns + "schema").FirstOrDefault();
            if (schema == null)
            {
                result.Errors.Add("No schema element found in WSDL");
                return result;
            }

            var complexTypes = schema.Elements(ns + "complexType").Count();
            result.ObjectCount = complexTypes;

            // Try to extract version info from comments or attributes
            var rootElement = doc.Root;
            if (rootElement != null)
            {
                var versionComment = doc.DescendantNodes()
                    .OfType<XComment>()
                    .FirstOrDefault(c => c.Value.Contains("version", StringComparison.OrdinalIgnoreCase));
                
                if (versionComment != null)
                {
                    result.SalesforceVersion = ExtractVersionFromComment(versionComment.Value);
                }
            }

            result.IsValid = !result.Errors.Any();
            
            if (result.ObjectCount == 0)
            {
                result.Warnings.Add("No complex types found in WSDL - this may not be a valid Salesforce Enterprise WSDL");
            }

            _logger.LogInformation("WSDL validation complete. Valid: {IsValid}, Objects: {Count}", 
                result.IsValid, result.ObjectCount);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Exception during validation: {ex.Message}");
            _logger.LogError(ex, "Error validating WSDL");
        }

        return result;
    }

    private MetadataObject? ExtractObjectFromComplexType(XElement complexType, XNamespace ns)
    {
        var typeName = complexType.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(typeName))
            return null;

        var metadataObject = new MetadataObject
        {
            ApiName = typeName,
            Label = GenerateLabelFromApiName(typeName),
            IsCustom = typeName.EndsWith("__c"),
            IsStandard = !typeName.EndsWith("__c"),
            LastSyncedAt = DateTime.UtcNow
        };

        // Extract fields from sequence/all elements
        var sequence = complexType.Descendants(ns + "sequence").FirstOrDefault() 
                      ?? complexType.Descendants(ns + "all").FirstOrDefault();

        if (sequence != null)
        {
            var elements = sequence.Elements(ns + "element").ToList();
            
            foreach (var element in elements)
            {
                var field = ExtractFieldFromElement(element);
                if (field != null)
                {
                    metadataObject.Fields.Add(field);

                    // If this is a reference field, create a relationship
                    if (!string.IsNullOrEmpty(field.ReferenceTo))
                    {
                        var relationship = new MetadataRelationship
                        {
                            FromField = field.ApiName,
                            ToObject = field.ReferenceTo,
                            RelationshipName = field.RelationshipName ?? field.ReferenceTo,
                            Cardinality = RelationshipCardinality.ManyToOne,
                            RelationshipType = RelationshipType.Lookup,
                            IsRequired = field.IsRequired
                        };
                        metadataObject.Relationships.Add(relationship);
                    }
                }
            }
        }

        // Set audit info based on common audit fields
        metadataObject.Audit = new AuditInfo
        {
            HasCreatedById = metadataObject.Fields.Any(f => f.ApiName == "CreatedById"),
            HasCreatedDate = metadataObject.Fields.Any(f => f.ApiName == "CreatedDate"),
            HasLastModifiedById = metadataObject.Fields.Any(f => f.ApiName == "LastModifiedById"),
            HasLastModifiedDate = metadataObject.Fields.Any(f => f.ApiName == "LastModifiedDate"),
            HasSystemModstamp = metadataObject.Fields.Any(f => f.ApiName == "SystemModstamp")
        };

        return metadataObject;
    }

    private MetadataField? ExtractFieldFromElement(XElement element)
    {
        var fieldName = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(fieldName))
            return null;

        var field = new MetadataField
        {
            ApiName = fieldName,
            Label = GenerateLabelFromApiName(fieldName),
            IsCustom = fieldName.EndsWith("__c"),
            IsNillable = element.Attribute("nillable")?.Value == "true"
        };

        // Extract data type
        var typeAttribute = element.Attribute("type")?.Value;
        if (!string.IsNullOrEmpty(typeAttribute))
        {
            field.DataType = MapXsdTypeToFieldType(typeAttribute);
            
            // Check if this is a reference field (ID type ending in "Id")
            if (fieldName.EndsWith("Id") && fieldName != "Id" && field.DataType == FieldDataType.Id)
            {
                // This is likely a reference field
                var referencedObject = fieldName.Substring(0, fieldName.Length - 2);
                field.ReferenceTo = referencedObject;
                field.RelationshipName = referencedObject;
                field.DataType = FieldDataType.Reference;
            }
        }

        // Extract length constraints
        var maxLength = element.Attribute("maxLength")?.Value;
        if (int.TryParse(maxLength, out int length))
        {
            field.Length = length;
        }

        // Set field capabilities based on data type
        field.Capabilities = DetermineFieldCapabilities(field.DataType);

        return field;
    }

    private FieldDataType MapXsdTypeToFieldType(string xsdType)
    {
        // Remove namespace prefix if present
        var typeName = xsdType.Contains(':') ? xsdType.Split(':')[1] : xsdType;

        return typeName.ToLower() switch
        {
            "string" => FieldDataType.String,
            "boolean" => FieldDataType.Boolean,
            "int" => FieldDataType.Int,
            "double" => FieldDataType.Double,
            "date" => FieldDataType.Date,
            "datetime" => FieldDataType.DateTime,
            "time" => FieldDataType.Time,
            "id" => FieldDataType.Id,
            "base64binary" => FieldDataType.Base64,
            _ => FieldDataType.AnyType
        };
    }

    private FieldCapabilities DetermineFieldCapabilities(FieldDataType dataType)
    {
        var capabilities = new FieldCapabilities
        {
            IsFilterable = true,
            IsSortable = true,
            IsGroupable = false,
            IsAggregatable = false
        };

        // Set default filter operations based on data type
        switch (dataType)
        {
            case FieldDataType.String:
            case FieldDataType.Email:
            case FieldDataType.Phone:
            case FieldDataType.Url:
            case FieldDataType.Textarea:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.Like,
                    FilterOperation.NotLike,
                    FilterOperation.Contains,
                    FilterOperation.StartsWith,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                };
                capabilities.IsGroupable = true;
                break;

            case FieldDataType.Int:
            case FieldDataType.Double:
            case FieldDataType.Currency:
            case FieldDataType.Percent:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.LessThan,
                    FilterOperation.LessThanOrEqual,
                    FilterOperation.GreaterThan,
                    FilterOperation.GreaterThanOrEqual,
                    FilterOperation.Between,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                };
                capabilities.IsAggregatable = true;
                capabilities.IsGroupable = true;
                capabilities.AggregateFunctions = new List<AggregateFunction>
                {
                    AggregateFunction.Count,
                    AggregateFunction.Sum,
                    AggregateFunction.Avg,
                    AggregateFunction.Min,
                    AggregateFunction.Max
                };
                break;

            case FieldDataType.Date:
            case FieldDataType.DateTime:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.LessThan,
                    FilterOperation.LessThanOrEqual,
                    FilterOperation.GreaterThan,
                    FilterOperation.GreaterThanOrEqual,
                    FilterOperation.Between,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                };
                capabilities.IsGroupable = true;
                capabilities.AggregateFunctions = new List<AggregateFunction>
                {
                    AggregateFunction.Count,
                    AggregateFunction.Min,
                    AggregateFunction.Max
                };
                break;

            case FieldDataType.Boolean:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals
                };
                capabilities.IsGroupable = true;
                capabilities.IsAggregatable = false;
                break;

            case FieldDataType.Picklist:
            case FieldDataType.MultiPicklist:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.In,
                    FilterOperation.NotIn
                };
                capabilities.IsGroupable = true;
                break;

            case FieldDataType.Reference:
            case FieldDataType.Id:
                capabilities.FilterOperations = new List<FilterOperation>
                {
                    FilterOperation.Equals,
                    FilterOperation.NotEquals,
                    FilterOperation.In,
                    FilterOperation.NotIn,
                    FilterOperation.IsNull,
                    FilterOperation.IsNotNull
                };
                capabilities.IsGroupable = true;
                break;
        }

        capabilities.SortOperations = new List<SortOperation>
        {
            SortOperation.Ascending,
            SortOperation.Descending
        };

        return capabilities;
    }

    private bool IsUtilityType(string typeName)
    {
        var utilityPrefixes = new[] 
        { 
            "Query", "Get", "Search", "Describe", "Delete", "Update", 
            "Create", "Upsert", "Merge", "Convert", "Empty", "Error",
            "Login", "Logout", "Session", "SOAP", "API"
        };

        var utilitySuffixes = new[] 
        { 
            "Result", "Request", "Response", "Header", "Options", 
            "Fault", "Type", "Status"
        };

        return utilityPrefixes.Any(p => typeName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || utilitySuffixes.Any(s => typeName.EndsWith(s, StringComparison.OrdinalIgnoreCase));
    }

    private string GenerateLabelFromApiName(string apiName)
    {
        // Remove __c suffix for custom fields/objects
        var name = apiName.EndsWith("__c") ? apiName.Substring(0, apiName.Length - 3) : apiName;

        // Insert spaces before capital letters
        var result = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();

        // Replace underscores with spaces
        result = result.Replace("_", " ");

        return result;
    }

    private string? ExtractVersionFromComment(string comment)
    {
        // Try to extract version number from comment
        var match = System.Text.RegularExpressions.Regex.Match(comment, @"v?(\d+\.\d+)");
        return match.Success ? match.Groups[1].Value : null;
    }
}
