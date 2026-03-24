using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Couture.Identity.Contracts;
using Couture.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Couture.Identity.Services;

public sealed class TokenService
{
    private readonly UserManager<CoutureUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public TokenService(UserManager<CoutureUser> userManager, IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime Expiry)> GenerateTokenAsync(CoutureUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new("FullName", user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));

            if (CoutureRoles.RolePermissions.TryGetValue(role, out var permissions))
            {
                foreach (var permission in permissions)
                {
                    if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                        claims.Add(new Claim("Permission", permission));
                }
            }
        }

        var expirationMinutes = user.SessionDurationHours > 0
            ? user.SessionDurationHours * 60
            : _jwtOptions.TokenExpirationInMinutes;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return (accessToken, refreshToken, expiry);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
