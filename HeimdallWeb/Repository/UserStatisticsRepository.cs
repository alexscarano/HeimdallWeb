using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Repository;

public class UserStatisticsRepository : IUserStatisticsRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserStatisticsRepository> _logger;

    public UserStatisticsRepository(AppDbContext context, ILogger<UserStatisticsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId)
    {
        try
        {
            var viewModel = new UserStatisticsViewModel
            {
                ScanSummary = await GetUserScanSummaryAsync(userId),
                FindingsSummary = await GetUserFindingsSummaryAsync(userId),
                RiskTrend = await GetUserRiskTrendAsync(userId),
                CategoryBreakdown = await GetUserCategoryBreakdownAsync(userId),
                RecentScans = await _context.History
                    .Where(h => h.user_id == userId && h.has_completed)
                    .OrderByDescending(h => h.created_date)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas do usuário {UserId}", userId);
            return new UserStatisticsViewModel();
        }
    }

    public async Task<UserScanSummary?> GetUserScanSummaryAsync(int userId)
    {
        try
        {
            return await _context.UserScanSummary
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.user_id == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de scans do usuário {UserId}", userId);
            return null;
        }
    }

    public async Task<UserFindingsSummary?> GetUserFindingsSummaryAsync(int userId)
    {
        try
        {
            return await _context.UserFindingsSummary
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.user_id == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de findings do usuário {UserId}", userId);
            return null;
        }
    }

    public async Task<List<UserRiskTrend>> GetUserRiskTrendAsync(int userId)
    {
        try
        {
            return await _context.UserRiskTrend
                .Where(t => t.user_id == userId)
                .OrderByDescending(t => t.risk_date)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tendência de riscos do usuário {UserId}", userId);
            return new List<UserRiskTrend>();
        }
    }

    public async Task<List<UserCategoryBreakdown>> GetUserCategoryBreakdownAsync(int userId)
    {
        try
        {
            return await _context.UserCategoryBreakdown
                .Where(c => c.user_id == userId)
                .OrderByDescending(c => c.category_count)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter breakdown de categorias do usuário {UserId}", userId);
            return new List<UserCategoryBreakdown>();
        }
    }
}
