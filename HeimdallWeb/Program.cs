using HeimdallWeb.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Centralized service registration
builder.Services.AddHeimdallServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

// Centralized pipeline configuration
app.UseHeimdallPipeline();

app.Run();
