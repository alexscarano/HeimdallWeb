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
                Expires = DateTime.UtcNow.AddHours(hours)
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
    }
}
