using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HeimdallWeb.Extensions
{
    public static class HostingExtensions
    {
        public static IServiceCollection AddHeimdallServices(this IServiceCollection services, IConfiguration config)
        {
            // Routing + MVC conventions
            services.RoutesSettings();

            // Persistence
            var connectionString = config.GetConnectionString("AppDbConnectionString");
            services.AddPersistence(connectionString);

            // app services & repositories
            services.AddRepositories();
            services.AddServices();

            // HttpContext accessor
            services.AddHttpContextAccessor();

            // JWT options binding
            services.Configure<JwtOptions>(config.GetSection("Jwt"));

            var jwtOptions = config.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
            var jwtKey = Encoding.ASCII.GetBytes(jwtOptions.Key ?? "Key não informada");

            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
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

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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
                        context.HandleResponse();
                        context.Response.Redirect("/Login/Index");
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.Redirect("/Home/AcessoRestrito");
                        return Task.CompletedTask;
                    }
                };
            });

            // Rate limiting configuration (global + named ScanPolicy)
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 85,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.AddPolicy<string>("ScanPolicy", httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 4,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.RejectionStatusCode = 429;
                options.OnRejected = async (context, ct) =>
                {
                    var logger = context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                    var log = logger?.CreateLogger("RateLimiter") ?? NullLogger.Instance;
                    try
                    {
                        // Log the rejection for diagnostics
                        log.LogWarning("Request rejected by rate limiter. Path={Path}, IP={IP}",
                            context.HttpContext.Request.Path, context.HttpContext.Connection.RemoteIpAddress?.ToString());

                        // Set explicit Set-Cookie header and return 302 to root with query param
                        var isHttps = context.HttpContext.Request.IsHttps;
                        var cookieValue = "rateLimited=1; Max-Age=60; Path=/; SameSite=Lax" + (isHttps ? "; Secure" : "");

                        // Use header append to be robust in proxy scenarios
                        context.HttpContext.Response.Headers.Append(HeaderNames.SetCookie, cookieValue);
                        context.HttpContext.Response.StatusCode = StatusCodes.Status302Found;
                        context.HttpContext.Response.Headers[HeaderNames.Location] = "/?rateLimited=1";

                        await context.HttpContext.Response.CompleteAsync();
                    }
                    catch (Exception ex)
                    {
                        // fallback to plain 429 response
                        log.LogError(ex, "Error while handling rate limit rejection");
                        context.HttpContext.Response.Headers.RetryAfter = "60";
                        context.HttpContext.Response.ContentType = "text/plain";
                        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", ct);
                    }
                };
            });

            return services;
        }

        public static WebApplication UseHeimdallPipeline(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Rate limiter must run after routing so endpoint metadata (named policies) is available
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }
    }
}
