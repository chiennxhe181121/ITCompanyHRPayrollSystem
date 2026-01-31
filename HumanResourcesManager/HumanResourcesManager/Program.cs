using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // DbContext
            builder.Services.AddDbContext<HumanManagerContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            // ===== BLL DI =====
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ===== SESSION =====
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // ===== SEED DATA =====
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                                   .GetRequiredService<HumanManagerContext>();
                SeedData.Initialize(context);
            }

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();        // 🔥 PHẢI CÓ
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
