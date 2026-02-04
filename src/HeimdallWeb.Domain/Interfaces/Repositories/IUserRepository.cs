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
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default);

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
}
