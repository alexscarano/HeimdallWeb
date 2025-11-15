using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;
using HeimdallWeb.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HeimdallWeb.Repository;

/// <summary>
/// Implementação do repositório de dashboard com cache em memória.
/// 
/// Por que usar VIEWs SQL?
/// - Performance: cálculos agregados são executados no banco (indexados e otimizados)
/// - Manutenibilidade: lógica de agregação centralizada no banco
/// - Reusabilidade: VIEWs podem ser consumidas por múltiplos endpoints
/// 
/// Por que usar cache?
/// - Estatísticas mudam lentamente (30s é aceitável)
/// - Atividade recente muda frequentemente (5s para frescor)
/// - Reduz carga no banco significativamente em dashboards com múltiplos usuários
/// 
/// Arquitetura:
/// - Todas as consultas usam AsNoTracking() (read-only, sem tracking do EF)
/// - Cache por tipo de dado (permite invalidação granular no futuro)
/// - Queries diretas às VIEWs via DbContext.Set<T>()
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardRepository> _logger;

    // Cache keys
    private const string CacheKey_UserStats = "Dashboard_UserStats";
    private const string CacheKey_ScanStats = "Dashboard_ScanStats";
    private const string CacheKey_LogsOverview = "Dashboard_LogsOverview";
    private const string CacheKey_RecentActivity = "Dashboard_RecentActivity";
    private const string CacheKey_ScanTrend = "Dashboard_ScanTrend";
    private const string CacheKey_UserRegTrend = "Dashboard_UserRegTrend";

    // Cache durations
    private static readonly TimeSpan StatsCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ActivityCacheDuration = TimeSpan.FromSeconds(5);

    public DashboardRepository(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<DashboardRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(int logPage = 1, int logPageSize = 10)
    {
        try
        {
            // Executar as consultas sequencialmente para evitar uso concorrente do mesmo DbContext
            var userStats = await GetUserStatsAsync();
            var scanStats = await GetScanStatsAsync();
            var logsOverview = await GetLogsOverviewAsync();
            var recentActivity = await GetRecentActivityAsync(logPage, logPageSize);
            var scanTrend = await GetScanTrendAsync();
            var userRegTrend = await GetUserRegistrationTrendAsync();

            return new AdminDashboardViewModel
            {
                UserStats = userStats,
                ScanStats = scanStats,
                LogsOverview = logsOverview,
                RecentActivity = recentActivity,
                ScanTrend = scanTrend,
                UserRegistrationTrend = userRegTrend
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados do dashboard administrativo");
            // Retornar modelo vazio em caso de erro
            return new AdminDashboardViewModel();
        }
    }

    private async Task<DashboardUserStats> GetUserStatsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_UserStats, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            _logger.LogInformation("Buscando UserStats do banco (cache miss)");
            var result = await _context.Set<DashboardUserStats>()
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? new DashboardUserStats();
            _logger.LogInformation("UserStats obtido: TotalUsers={Total}, Active={Active}, Blocked={Blocked}", 
                result.total_users, result.active_users, result.blocked_users);
            return result;
        }) ?? new DashboardUserStats();
    }

    private async Task<DashboardScanStats> GetScanStatsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_ScanStats, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<DashboardScanStats>()
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? new DashboardScanStats();
        }) ?? new DashboardScanStats();
    }

    private async Task<DashboardLogsOverview> GetLogsOverviewAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_LogsOverview, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<DashboardLogsOverview>()
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? new DashboardLogsOverview();
        }) ?? new DashboardLogsOverview();
    }

    private async Task<PaginatedResult<DashboardRecentActivity>> GetRecentActivityAsync(int page, int pageSize)
    {
        // Não usar cache para atividade recente paginada (diferente por página)
        var query = _context.Set<DashboardRecentActivity>().AsNoTracking();
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<DashboardRecentActivity>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<List<DashboardScanTrendDaily>> GetScanTrendAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_ScanTrend, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<DashboardScanTrendDaily>()
                .AsNoTracking()
                .ToListAsync();
        }) ?? new List<DashboardScanTrendDaily>();
    }

    private async Task<List<DashboardUserRegistrationTrend>> GetUserRegistrationTrendAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_UserRegTrend, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<DashboardUserRegistrationTrend>()
                .AsNoTracking()
                .ToListAsync();
        }) ?? new List<DashboardUserRegistrationTrend>();
    }

    public Task<UserMetricsViewModel?> GetUserMetricsAsync(int userId)
    {
        // Placeholder - implementar no futuro
        _logger.LogInformation("GetUserMetricsAsync chamado para userId={UserId} (não implementado)", userId);
        return Task.FromResult<UserMetricsViewModel?>(null);
    }
}
