using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HeimdallWeb.Helpers
{
    public static class ExtentionsHelpers
    {
        /// <summary>
        /// método de extensão para "excluir propriedades" via reflexão
        /// (funciona em memória, mas não é traduzido para SQL pelo EF Core):
        /// var result = users
        /// .Select(u => u.Exclude("Password", "Token")).ToList();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static object Exclude<T>(this T obj, params string[] props)
        {
            var dict = typeof(T).GetProperties()
                .Where(p => !props.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p.GetValue(obj));

            return dict;
        }

        /// <summary>
        /// Transforma o objeto em JSON formatado (com indentação)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Objeto em forma de JSON</returns>
        public static string ToJson(this object obj)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };

            return System.Text.Json.JsonSerializer.Serialize(obj, options);
        }
    }
}
