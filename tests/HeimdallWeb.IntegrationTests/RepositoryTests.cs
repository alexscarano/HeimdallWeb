using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Infrastructure.Data;
using HeimdallWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit; // Added for [Fact] and Assert

namespace HeimdallWeb.IntegrationTests;

public class RepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;

    public RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=db_heimdall;Username=postgres;Password=root")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Fact] // Added [Fact] attribute
    public async Task UserRepository_CRUD_Success()
    {
        var repo = new UserRepository(_context);

        // CREATE
        var email = EmailAddress.Create("newuser@example.com");
        var user = new User( // Corrected: calling constructor directly
            username: "newuser",
            email: email,
            passwordHash: "hashed_password_123",
            userType: UserType.Default
        );

        var created = await repo.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.Entry(created).State = EntityState.Detached; // Detach the added entity to prevent tracking conflicts

        Assert.NotEqual(0, created.UserId);

        // READ by ID
        var fetched = await repo.GetByIdAsync(created.UserId);
        Assert.NotNull(fetched);
        Assert.Equal("newuser", fetched.Username);

        // READ by Email
        var fetchedByEmail = await repo.GetByEmailAsync(email);
        Assert.NotNull(fetchedByEmail);
        Assert.Equal(created.UserId, fetchedByEmail.UserId);

        // UPDATE
        fetched!.UpdatePassword("new_hashed_password"); // Null-forgiving operator added
        await repo.UpdateAsync(fetched);
        await _context.SaveChangesAsync();

        var updated = await repo.GetByIdAsync(created.UserId);
        Assert.Equal("new_hashed_password", updated.PasswordHash);

        // EXISTS check
        var exists = await repo.ExistsByEmailAsync(email);
        Assert.True(exists);

        // GET ALL
        var all = await repo.GetAllAsync();
        Assert.Contains(all, u => u.UserId == created.UserId);
    }
    
    [Fact]
    public async Task ScanHistoryRepository_JSONB_Success()
    {
        var repo = new ScanHistoryRepository(_context);
        var userRepo = new UserRepository(_context);

        // Create user first
        var user = await userRepo.AddAsync(new User( // Corrected: calling constructor directly
            username: "newuser",
            email: EmailAddress.Create("alexandrescarano@gmail.com"),
            passwordHash: "hashed_password_123",
            userType: UserType.Default
        ));
        
        await _context.SaveChangesAsync();

        // Create scan with JSONB
        var target = ScanTarget.Create("https://jsonb-test.com");
        var duration = ScanDuration.Create(TimeSpan.FromSeconds(45));

        var scan = new ScanHistory(
            target: target,
            userId: user.UserId
        );
        
        scan.CompleteScan(duration, "JSONB test summary", "eu dou a bunda para vários homens");

        var created = await repo.AddAsync(scan);
        await _context.SaveChangesAsync();

        // Fetch and verify JSONB
        var fetched = await repo.GetByIdAsync(created.HistoryId);
        Assert.Contains("jsonb_data", fetched.RawJsonResult);

        // Test JSONB query (if implemented in repository)
        // var byTarget = await repo.GetByTargetAsync(target);
        // Assert.NotEmpty(byTarget);
    }
}