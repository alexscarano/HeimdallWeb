using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HeimdallWeb.Helpers
{
    public static class LinqHelpers
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
    }
}
