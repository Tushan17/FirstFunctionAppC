using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FirstFunctionApp.Services;

public class JwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt__Secret is not configured.");
        _issuer = configuration["Jwt:Issuer"] ?? "FirstFunctionApp";
        _audience = configuration["Jwt:Audience"] ?? "FirstFunctionAppUsers";
        _expirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 60;
    }

    public (string Token, DateTime Expires) GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public (bool IsValid, ClaimsPrincipal? Principal, string? Error) ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, GetValidationParameters(), out _);
            return (true, principal, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    private TokenValidationParameters GetValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
        ValidateIssuer = true,
        ValidIssuer = _issuer,
        ValidateAudience = true,
        ValidAudience = _audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
}
