using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace HeimdallWeb.Helpers
{
    public static class CookiesHelper
    {
        private const string cookieName = "authHeimdallCookie";

        public static void generateAuthCookie(HttpResponse response, string token, int hours = 12)
        {
            response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, //trocar para true para producao
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(hours)
            });
        }

        public static string? getAuthCookie(HttpRequest request)
        {
            return request.Cookies.TryGetValue(cookieName, out var token) ? token : null;
        }

        public static void deleteAuthCookie(HttpResponse response)
        {
            response.Cookies.Delete(cookieName);
        }
    
        public static int getUserIDFromCookie(string? cookie)
        {
            if (cookie == null) return -1;

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var principal = handler.ReadJwtToken(cookie) as JwtSecurityToken;
            // faz query no cookie tentando capturar o claim "sub" (subject) que é o user_id no bd
            var query = principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return int.Parse(query ?? string.Empty);
        }
    }
}
