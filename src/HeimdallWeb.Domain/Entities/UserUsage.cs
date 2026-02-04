using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// UserUsage entity representing daily request tracking for rate limiting and quota management.
/// </summary>
public class UserUsage
{
    public int UserUsageId { get; private set; }
    public DateTime Date { get; private set; }
    public int RequestCounts { get; private set; }
    public int UserId { get; private set; }

    // Navigation property (parent)
    public User? User { get; private set; }

    // Parameterless constructor for EF Core
    private UserUsage() { }

    /// <summary>
    /// Creates a new UserUsage instance.
    /// </summary>
    public UserUsage(int userId, DateTime date)
    {
        if (userId <= 0)
            throw new ValidationException("User ID must be positive.");

        UserId = userId;
        Date = date.Date; // Normalize to date only (no time component)
        RequestCounts = 0;
    }

    /// <summary>
    /// Increments the request count by 1.
    /// </summary>
    public void IncrementRequests()
    {
        RequestCounts++;
    }

    /// <summary>
    /// Increments the request count by a specific amount.
    /// </summary>
    public void IncrementRequests(int count)
    {
        if (count <= 0)
            throw new ValidationException("Increment count must be positive.");

        RequestCounts += count;
    }

    /// <summary>
    /// Resets the request count to zero.
    /// </summary>
    public void ResetRequests()
    {
        RequestCounts = 0;
    }
}
