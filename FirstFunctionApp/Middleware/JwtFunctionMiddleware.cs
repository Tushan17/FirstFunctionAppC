using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using FirstFunctionApp.Services;

namespace FirstFunctionApp.Middleware;

// Functions whose invocations bypass JWT authentication
internal static class PublicFunctions
{
    internal static readonly HashSet<string> Names = ["GetToken"];
}

public class JwtFunctionMiddleware : IFunctionsWorkerMiddleware
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<JwtFunctionMiddleware> _logger;

    public JwtFunctionMiddleware(JwtTokenService jwtTokenService, ILogger<JwtFunctionMiddleware> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (PublicFunctions.Names.Contains(context.FunctionDefinition.Name))
        {
            await next(context);
            return;
        }

        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            await next(context);
            return;
        }

        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Missing or malformed Authorization header on request to {FunctionName}",
                context.FunctionDefinition.Name);
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Unauthorized: a valid Bearer token is required."
            });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var (isValid, principal, error) = _jwtTokenService.ValidateToken(token);

        if (!isValid)
        {
            _logger.LogWarning("JWT validation failed for request to {FunctionName}: {Error}",
                context.FunctionDefinition.Name, error);
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Unauthorized: invalid or expired token."
            });
            return;
        }

        httpContext.User = principal!;
        await next(context);
    }
}
