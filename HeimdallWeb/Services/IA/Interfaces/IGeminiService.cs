namespace HeimdallWeb.Services.IA.Interfaces
{
    public interface IGeminiService
    {
        public Task<string> GeneratePrompt(string jsonInput);
    }
}
