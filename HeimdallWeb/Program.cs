using HeimdallWeb.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind Jwt options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var jwtKey = Encoding.ASCII.GetBytes(jwtOptions.Key ?? "Key não informada");

var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

// Exemplo para pegar sempre em Brasília:
builder.Services.AddHttpContextAccessor();

// injeção de dependencia
builder.Services.AddRepositories();
builder.Services.AddServices();

// capturar string de conexão sql
builder.Services.AddPersistence(connectionString);

// config jwt auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata; // em prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("authHeimdallCookie"))
            {
                context.Token = context.Request.Cookies["authHeimdallCookie"];
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // se não autenticado -> redireciona pro login
            context.HandleResponse();
            context.Response.Redirect("/Login/Index");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            // se autenticado mas sem permissão -> redireciona
            context.Response.Redirect("/Home/AcessoRestrito");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.ConfigurePipeline();

app.Run();
