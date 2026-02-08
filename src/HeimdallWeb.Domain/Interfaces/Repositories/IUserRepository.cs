using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by their unique identifier FOR UPDATE (with tracking).
    /// Use this method when you need to modify the entity.
    /// </summary>
    Task<User?> GetByIdForUpdateAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Gets all users in the system.
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user exists with the given email address.
    /// </summary>
    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user exists with the given username.
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by username, excluding a specific user ID (for duplicate detection).
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, int excludeUserId, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by email, excluding a specific user ID (for duplicate detection).
    /// </summary>
    Task<User?> GetByEmailAsync(EmailAddress email, int excludeUserId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    Task DeleteAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated users with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="searchTerm">Search in username or email</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isAdmin">Filter by user type (true = admin, false = regular)</param>
    /// <param name="createdFrom">Filter by created date (from)</param>
    /// <param name="createdTo">Filter by created date (to)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple with users list and total count</returns>
    Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        bool? isAdmin = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        CancellationToken ct = default);
}
