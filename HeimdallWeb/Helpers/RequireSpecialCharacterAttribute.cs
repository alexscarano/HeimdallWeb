using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HeimdallWeb.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequireSpecialCharacterAttribute : ValidationAttribute
    {
        public RequireSpecialCharacterAttribute()
        {
            // Mensagem padrão; pode ser sobrescrita via ErrorMessage
            ErrorMessage = "A senha deve conter pelo menos um caractere especial, uma letra maiscula.";
        }

        public override bool IsValid(object?  value) //sim
        {
            if (value is null) return true; // Required é tratado separadamente

            string password = value.ToString() ?? string.Empty;

            // Regex que verifica qualquer caractere NÃO alfanumérico
            return Regex.IsMatch(password, @"^(?=.*[^A-Za-z0-9])(?=.*[A-Za-z0-9])(?=.*[A-Z]).{8,}$");
        }
    }
}

