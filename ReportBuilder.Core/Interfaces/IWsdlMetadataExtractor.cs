using ReportBuilder.Core;

namespace ReportBuilder.Core;

/// <summary>
/// Service for extracting metadata from Salesforce Enterprise WSDL
/// </summary>
public interface IWsdlMetadataExtractor
{
    /// <summary>
    /// Extract all metadata from WSDL file
    /// </summary>
    /// <param name="wsdlPath">Path to the WSDL file</param>
    /// <returns>List of all objects with complete metadata</returns>
    Task<List<MetadataObject>> ExtractFromWsdlAsync(string wsdlPath);

    /// <summary>
    /// Extract metadata from WSDL stream
    /// </summary>
    /// <param name="wsdlStream">Stream containing WSDL content</param>
    /// <returns>List of all objects with complete metadata</returns>
    Task<List<MetadataObject>> ExtractFromWsdlStreamAsync(Stream wsdlStream);

    /// <summary>
    /// Extract a specific object from WSDL
    /// </summary>
    /// <param name="wsdlPath">Path to the WSDL file</param>
    /// <param name="objectApiName">API name of the object to extract</param>
    /// <returns>Single object with complete metadata</returns>
    Task<MetadataObject?> ExtractObjectFromWsdlAsync(string wsdlPath, string objectApiName);

    /// <summary>
    /// Validate WSDL file structure
    /// </summary>
    /// <param name="wsdlPath">Path to the WSDL file</param>
    /// <returns>Validation result</returns>
    Task<WsdlValidationResult> ValidateWsdlAsync(string wsdlPath);
}

/// <summary>
/// Result of WSDL validation
/// </summary>
public class WsdlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int ObjectCount { get; set; }
    public string? SalesforceVersion { get; set; }
    public DateTime? GeneratedDate { get; set; }
}
