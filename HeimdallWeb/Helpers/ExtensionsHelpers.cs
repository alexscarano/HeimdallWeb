using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Helpers
{
    public static class ExtensionsHelpers
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
        /// Transforma o objeto em JSON formatado (NewtonSoft)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Objeto em forma de JSON</returns>
        public static JObject ToJson(this object? obj)
        {
            if (obj is null)
                return new JObject();

            // Se já for um JObject, retorna direto
            if (obj is JObject jObj)
                return jObj;

            // Se for uma string, tenta interpretar como JSON
            if (obj is string jsonString)
            {
                try
                {
                    return JObject.Parse(jsonString);
                }
                catch
                {
                    // Caso a string não seja JSON válido, cria um JObject com valor bruto
                    return new JObject { ["value"] = jsonString };
                }
            }

            // Se for qualquer outro tipo de objeto .NET
            return JObject.FromObject(obj);
        }
    }
}
