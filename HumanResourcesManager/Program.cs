using HumanResourcesManager.Auth;
using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC services
            builder.Services.AddControllersWithViews();

            // Database configuration
            builder.Services.AddDbContext<HumanManagerContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            // Authentication configuration (Cookie + Google)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/HumanResourcesManager/login";
                options.AccessDeniedPath = "/HumanResourcesManager/denied";

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.Redirect("/HumanResourcesManager/login");
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = ctx =>
                    {
                        ctx.Response.Redirect("/HumanResourcesManager/login");
                        return Task.CompletedTask;
                    }
                };
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });
            //.AddGoogle(options =>
            //{
            //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
            //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
            //    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //});

            // Role-based authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ADMIN", p => p.RequireRole("ADMIN"));
                options.AddPolicy("HR", p => p.RequireRole("HR"));
                options.AddPolicy("EMP", p => p.RequireRole("EMP"));
                options.AddPolicy("MANAGER", p => p.RequireRole("MANAGER"));
            });

            // Dependency Injection: Repositories (DAL)
            builder.Services.AddScoped<CookieAuthManager>();

            builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            // Dependency Injection: Business Services (BLL)
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<OtpService>();
            builder.Services.AddSingleton<EmailService>();

            builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

            builder.Services.AddScoped<IUserAccountService, UserAccountService>(); // 08/02/2026



            // Session configuration
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Email settings configuration
            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );

            // Access HttpContext in services
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Database seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<HumanManagerContext>();
                    SeedData.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error seeding database");
                }
            }

            // HTTP pipeline configuration
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Middleware order: Session -> Auth -> Authorization
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // Default route mapping
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}