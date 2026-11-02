using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Nythorion.Api.IntegrationTests.Infrastructure;

public static class TokenHelper
{
    private const string Secret = "test-secret-key-nythorion-integration-tests-32chars";
    public static readonly SymmetricSecurityKey SecurityKey = new(Encoding.UTF8.GetBytes(Secret));

    public static string GenerateToken(string userId, string? username = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Name, username ?? userId)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
