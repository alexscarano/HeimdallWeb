using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeimdallWeb.Models;
using Microsoft.IdentityModel.Tokens;

namespace HeimdallWeb.Helpers
{
    public static class TokenService
    {
        public static string generateToken(UserModel user, IConfiguration config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            byte[] key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? "Chave JWT não capturada");
            if (key.Length < 32)
                throw new Exception("A chave JWT deve ser no mínimo de 32 caracteres");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.user_id.ToString()),
                    new Claim(ClaimTypes.Name, user.username),
                    new Claim(ClaimTypes.Email, user.email),
                    new Claim(ClaimTypes.Role, user.user_type.ToString()) // admin ou default
                }),
                Expires = DateTime.Now.AddHours(12),
                SigningCredentials = new SigningCredentials(new
                        SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static int? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst(ClaimTypes.Name)?.Value;

            if (int.TryParse(sub, out var id)) return id;
            return null;
        }
    }
}
