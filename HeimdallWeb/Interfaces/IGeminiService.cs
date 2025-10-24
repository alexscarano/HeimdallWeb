namespace HeimdallWeb.Interfaces
{
    public interface IGeminiService
    {
        public Task<string> GeneratePrompt(string jsonInput);
    }
}
