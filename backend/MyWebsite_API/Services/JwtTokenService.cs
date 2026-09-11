using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _key = configuration["JwtSettings:Key"]
        ?? throw new InvalidOperationException("JwtSettings:Key is not configured.");

    private readonly string _issuer = configuration["JwtSettings:Issuer"] ?? "IDSProductsPortal";
    private readonly string _audience = configuration["JwtSettings:Audience"] ?? "IDSProductsPortal.Client";
    private readonly int _expirationMinutes = configuration.GetValue("JwtSettings:ExpirationMinutes", 60);

    public AuthResponse CreateToken(UserAccount user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            User = user.ToResponse()
        };
    }
}
