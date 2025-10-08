namespace HeimdallWeb.DTO
{
    public class MenuViewDTO
    {
        public bool isAuthenticated { get; set; }
        public string? username { get; set; }
        public List<string> roles { get; set; } = new();
    }
}
