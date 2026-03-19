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
        _logger.LogInformation("Processing XML upload request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new XmlValidationResponse
                {
                    Success = false,
                    Message = "Request body is empty"
                });
            }

            var uploadRequest = JsonConvert.DeserializeObject<XmlUploadRequest>(requestBody);

            if (uploadRequest == null || string.IsNullOrWhiteSpace(uploadRequest.XmlContent))
            {
                return new BadRequestObjectResult(new XmlValidationResponse
                {
                    Success = false,
                    Message = "XmlContent is required in the request body"
                });
            }

            var (isValid, errors) = _validationService.ValidateXml(uploadRequest.XmlContent);

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

            var dataRoot = _processingService.ProcessXmlData(uploadRequest.XmlContent);

            if (dataRoot == null)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("XML processed successfully");

            return new OkObjectResult(new XmlValidationResponse
            {
                Success = true,
                Message = "XML validated and processed successfully",
                Data = dataRoot
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing XML upload");
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