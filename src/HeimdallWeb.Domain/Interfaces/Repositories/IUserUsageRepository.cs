using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserUsage entity operations.
/// </summary>
public interface IUserUsageRepository
{
    /// <summary>
    /// Gets the user usage record for a specific user and date.
    /// </summary>
    Task<UserUsage?> GetByUserAndDateAsync(int userId, DateTime date, CancellationToken ct = default);

    /// <summary>
    /// Adds a new user usage record.
    /// </summary>
    Task<UserUsage> AddAsync(UserUsage usage, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user usage record.
    /// </summary>
    Task UpdateAsync(UserUsage usage, CancellationToken ct = default);
}
