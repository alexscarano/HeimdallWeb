namespace HeimdallWeb.Helpers
{
    public static class GeminiHelper
    {
        /// <summary>
        /// Remover markdown do retorno da IA, para o JSON não quebrar
        /// </summary>
        /// <param name="json"></param>
        /// <returns>JSON formatado</returns>
        public static string RemoveMarkdown(this string json)
        {
			try
			{
                json = json.Replace("```json", "");
                json = json.Replace("```", "");

                return json;
            }
			catch (Exception) 
            {
                return json; 
            }

        }
    }
}
