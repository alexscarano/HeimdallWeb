using System.Globalization;
using System.Text;
using HeimdallWeb.Data;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "Key não informada");

var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

// Exemplo para pegar sempre em Brasília:
builder.Services.AddHttpContextAccessor();

// injeção de dependencia
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IFindingRepository, FindingRepository>();


// capturar string de conexão sql
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), o => o.CommandTimeout(90)));

// config jwt auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // em prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
   
app.Run();
