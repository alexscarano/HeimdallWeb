using System.Security.Cryptography;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Infrastructure.Data;

/// <summary>
/// Responsible for seeding initial data that the application requires to function.
/// Idempotent: safe to call on every startup. Creates data only when it is absent.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds required data into the database.
    /// Call this after app.Build() but before app.Run() in Program.cs.
    /// </summary>
    /// <param name="services">The application's IServiceProvider.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await SeedAdminUserAsync(db, logger);
    }

    // -------------------------------------------------------------------------
    // Admin user seed
    // -------------------------------------------------------------------------

    private static async Task SeedAdminUserAsync(AppDbContext db, ILogger logger)
    {
        // Guard: skip if ANY admin user already exists.
        bool adminExists = await db.Users
            .AnyAsync(u => u.UserType == UserType.Admin);

        if (adminExists)
        {
            logger.LogInformation("[DatabaseSeeder] Admin user already exists. Skipping seed.");
            return;
        }

        // Read the password from the environment, falling back to the default.
        string rawPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
                             ?? "Admin@123";

        string passwordHash = HashPassword(rawPassword);

        var email = EmailAddress.Create("admin@heimdall.local");

        var admin = new User(
            username: "heimadmin",
            email: email,
            passwordHash: passwordHash,
            userType: UserType.Admin
        );

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "[DatabaseSeeder] Admin user created. Username: {Username} | Email: {Email}",
            admin.Username,
            email.Value);
    }

    // -------------------------------------------------------------------------
    // Password hashing — PBKDF2/SHA-256, 100 000 iterations, 32-byte output.
    // Identical algorithm to HeimdallWeb.Application.Helpers.PasswordUtils.
    // Duplicated here intentionally: Infrastructure must not reference Application.
    // -------------------------------------------------------------------------

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        // Use the static Pbkdf2 method (preferred over the obsolete constructor in .NET 10).
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 100_000, HashAlgorithmName.SHA256, outputLength: 32);

        // Format: "<base64-salt>:<base64-hash>"  (same format as PasswordUtils.HashPassword)
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
