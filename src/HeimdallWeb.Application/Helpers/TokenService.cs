using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeimdallWeb.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HeimdallWeb.Application.Helpers;

public static class TokenService
{
    public static string GenerateToken(User user, IConfiguration config)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        byte[] key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
        if (key.Length < 32)
            throw new InvalidOperationException("JWT Key must be at least 32 characters");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email.Value),
                new Claim(ClaimTypes.Role, ((int)user.UserType).ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(12),
            Issuer = config["Jwt:Issuer"] ?? "HeimdallWeb",
            Audience = config["Jwt:Audience"] ?? "HeimdallWebUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public static Guid? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(sub, out var id))
            return id;

        return null;
    }
}
