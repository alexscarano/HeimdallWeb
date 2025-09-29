namespace HeimdallWeb.Enums
{
    public static class EnumMapper
    {
        public static string MapStatus(this SeverityLevel statusValue)
        {
            return statusValue switch
            {
                SeverityLevel.Low => "Baixo",
                SeverityLevel.Medium => "Médio",
                SeverityLevel.High => "Alto",
                SeverityLevel.Critical => "Crítico",
                _ => "Desconhecido"
            };
        }
    }
}