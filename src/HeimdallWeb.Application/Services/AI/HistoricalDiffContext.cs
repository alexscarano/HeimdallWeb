namespace HeimdallWeb.Application.Services.AI;

public record CategoryHistoryEntry(string Categoria, string Risco, int PresenteHaScans);

public record HistoricalDiffContext(IEnumerable<CategoryHistoryEntry> CategoriasAnteriores)
{
    public bool HasHistory => CategoriasAnteriores.Any();
}
