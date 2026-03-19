using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FirstFunctionApp.Models;
using FirstFunctionApp.Services;
using Newtonsoft.Json;

namespace FirstFunctionApp.Functions;

public class AuthFunction
{
    private readonly ILogger<AuthFunction> _logger;
    private readonly JwtTokenService _jwtTokenService;
    private readonly string _apiUsername;
    private readonly string _apiPassword;

    public AuthFunction(ILogger<AuthFunction> logger, JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _apiUsername = configuration["Api:Username"]
            ?? throw new InvalidOperationException("Api__Username is not configured.");
        _apiPassword = configuration["Api:Password"]
            ?? throw new InvalidOperationException("Api__Password is not configured.");
    }

    [Function("GetToken")]
    public async Task<IActionResult> GetToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/token")] HttpRequest req)
    {
        _logger.LogInformation("Token request received.");

        try
        {
            string body;
            using (var reader = new StreamReader(req.Body))
                body = await reader.ReadToEndAsync();

            var request = JsonConvert.DeserializeObject<TokenRequest>(body);

            if (request == null
                || string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return new BadRequestObjectResult(new
                {
                    success = false,
                    message = "Username and password are required."
                });
            }

            if (request.Username != _apiUsername || request.Password != _apiPassword)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return new UnauthorizedObjectResult(new { success = false, message = "Invalid credentials." });
            }

            var (token, expires) = _jwtTokenService.GenerateToken(request.Username);
            _logger.LogInformation("Token issued for user {Username}", request.Username);

            return new OkObjectResult(new TokenResponse { Token = token, Expires = expires });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return new ObjectResult(new { success = false, message = "Internal server error." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
