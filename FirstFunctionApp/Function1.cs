using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FirstFunctionApp.Models;
using FirstFunctionApp.Services;
using Newtonsoft.Json;

namespace FirstFunctionApp;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly XmlValidationService _validationService;
    private readonly XmlProcessingService _processingService;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;

        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Schemas", "DataSchema.xsd");
        _validationService = new XmlValidationService(schemaPath);
        _processingService = new XmlProcessingService();
    }

    [Function("Function1")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }

    [Function("UploadXml")]
    public async Task<IActionResult> UploadXml(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Processing XML upload request from Business Central.");

        try
        {
            // Read the request body with UTF-8 encoding to handle Business Central outstream
            string requestBody;
            using (var reader = new StreamReader(req.Body, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Received empty request body");
                return new BadRequestObjectResult(new XmlValidationResponse
                {
                    Success = false,
                    Message = "Request body is empty"
                });
            }

            // Remove BOM if present (Business Central often includes it)
            requestBody = requestBody.TrimStart('\uFEFF', '\u200B');

            _logger.LogInformation("Request body length: {Length} characters, Content-Type: {ContentType}", 
                requestBody.Length, req.ContentType);

            string xmlContent;

            // Detect if the request is raw XML or JSON-wrapped
            // Business Central typically sends raw XML via outstream
            var contentType = req.ContentType?.ToLowerInvariant() ?? string.Empty;

            if (contentType.Contains("xml") || requestBody.TrimStart().StartsWith("<?xml") || requestBody.TrimStart().StartsWith("<"))
            {
                // Raw XML from Business Central outstream
                _logger.LogInformation("Detected raw XML format (Business Central outstream)");
                xmlContent = requestBody;
            }
            else
            {
                // JSON-wrapped XML format
                _logger.LogInformation("Detected JSON format, attempting to deserialize");
                try
                {
                    var uploadRequest = JsonConvert.DeserializeObject<XmlUploadRequest>(requestBody);

                    if (uploadRequest == null || string.IsNullOrWhiteSpace(uploadRequest.XmlContent))
                    {
                        _logger.LogWarning("JSON deserialization resulted in null or empty XmlContent");
                        return new BadRequestObjectResult(new XmlValidationResponse
                        {
                            Success = false,
                            Message = "XmlContent is required in the request body"
                        });
                    }

                    xmlContent = uploadRequest.XmlContent;
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to parse JSON request body");
                    return new BadRequestObjectResult(new XmlValidationResponse
                    {
                        Success = false,
                        Message = $"Invalid JSON format: {jsonEx.Message}"
                    });
                }
            }

            _logger.LogInformation("Validating XML content (length: {Length} characters)", xmlContent.Length);

            var (isValid, errors) = _validationService.ValidateXml(xmlContent);

            if (!isValid)
            {
                _logger.LogWarning("XML validation failed with {ErrorCount} errors", errors.Count);
                return new BadRequestObjectResult(new XmlValidationResponse
                {
                    Success = false,
                    Message = "XML validation failed",
                    ValidationErrors = errors
                });
            }

            _logger.LogInformation("XML validation successful, processing data");

            var dataRoot = _processingService.ProcessXmlData(xmlContent);

            if (dataRoot == null)
            {
                _logger.LogError("XML processing returned null result");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("XML processed successfully from Business Central");

            return new OkObjectResult(new XmlValidationResponse
            {
                Success = true,
                Message = "XML validated and processed successfully",
                Data = dataRoot
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing XML upload from Business Central");
            return new ObjectResult(new XmlValidationResponse
            {
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}