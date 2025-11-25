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
    private const string CacheKey_IASummaryStats = "Dashboard_IASummaryStats";
    private const string CacheKey_RiskDistribution = "Dashboard_RiskDistribution";
    private const string CacheKey_TopCategories = "Dashboard_TopCategories";

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

    public async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(
        int logPage = 1, 
        int logPageSize = 10,
        string? logLevel = null,
        DateTime? logStartDate = null,
        DateTime? logEndDate = null)
    {
        try
        {
            // Executar as consultas sequencialmente para evitar uso concorrente do mesmo DbContext
            var userStats = await GetUserStatsAsync();
            var scanStats = await GetScanStatsAsync();
            var logsOverview = await GetLogsOverviewAsync();
            var recentActivity = await GetRecentActivityAsync(logPage, logPageSize, logLevel, logStartDate, logEndDate);
            var scanTrend = await GetScanTrendAsync();
            var userRegTrend = await GetUserRegistrationTrendAsync();

            // Buscar novos dados de IA Summary
            var iaSummaryStats = await GetIASummaryStatsAsync();
            var riskDistribution = await GetRiskDistributionDailyAsync();
            var topCategories = await GetTopCategoriesAsync();
            var vulnerableTargets = await GetMostVulnerableTargetsAsync();

            return new AdminDashboardViewModel
            {
                UserStats = userStats,
                ScanStats = scanStats,
                LogsOverview = logsOverview,
                RecentActivity = recentActivity,
                ScanTrend = scanTrend,
                UserRegistrationTrend = userRegTrend,
                IASummaryStats = iaSummaryStats,
                RiskDistribution = riskDistribution,
                TopCategories = topCategories,
                MostVulnerableTargets = vulnerableTargets
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

    private async Task<PaginatedResult<DashboardRecentActivity>> GetRecentActivityAsync(
        int page, 
        int pageSize,
        string? logLevel = null,
        DateTime? logStartDate = null,
        DateTime? logEndDate = null)
    {
        // Não usar cache para atividade recente paginada (diferente por página e filtros)
        var query = _context.Set<DashboardRecentActivity>().AsNoTracking();

        // Aplicar filtros conforme LogRepository
        if (!string.IsNullOrWhiteSpace(logLevel))
            query = query.Where(l => l.level == logLevel);

        if (logStartDate.HasValue)
        {
            var s = logStartDate.Value.Date;
            query = query.Where(l => l.timestamp >= s);
        }

        if (logEndDate.HasValue)
        {
            var eExclusive = logEndDate.Value.Date.AddDays(1);
            query = query.Where(l => l.timestamp < eExclusive);
        }
        
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

    private async Task<AdminIASummaryStats?> GetIASummaryStatsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_IASummaryStats, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<AdminIASummaryStats>()
                .AsNoTracking()
                .FirstOrDefaultAsync();
        });
    }

    private async Task<List<AdminRiskDistributionDaily>> GetRiskDistributionDailyAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_RiskDistribution, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<AdminRiskDistributionDaily>()
                .AsNoTracking()
                .OrderByDescending(r => r.risk_date)
                .ToListAsync();
        }) ?? new List<AdminRiskDistributionDaily>();
    }

    private async Task<List<AdminTopCategory>> GetTopCategoriesAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey_TopCategories, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatsCacheDuration;
            return await _context.Set<AdminTopCategory>()
                .AsNoTracking()
                .ToListAsync();
        }) ?? new List<AdminTopCategory>();
    }

    private async Task<List<AdminMostVulnerableTarget>> GetMostVulnerableTargetsAsync()
    {
        // Não usar cache para alvos vulneráveis (muda frequentemente)
        return await _context.Set<AdminMostVulnerableTarget>()
            .AsNoTracking()
            .ToListAsync();
    }
}
