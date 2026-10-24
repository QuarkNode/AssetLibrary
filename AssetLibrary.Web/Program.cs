namespace AssetLibrary
{
    using AssetLibrary.Data;
    using AssetLibrary.Data.Models;
    using AssetLibrary.Extensions;
    using AssetLibrary.Web.Infrastructure;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            string connectionString = GetConnectionString(builder);

            // Add services to the container.
            builder.Services.AddApplicationServices();

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AssetLibraryDbContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });


            builder.Services.AddIdentity<User, IdentityRole<int>>(opt =>
            {
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireDigit = false;
                opt.User.RequireUniqueEmail = true;
            }
            ).AddEntityFrameworkStores<AssetLibraryDbContext>();

            builder.Services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/Login";
                opt.AccessDeniedPath = "/Account/Login";
            });

            WebApplication app = builder.Build();

            // Seeding the Application with pre-existing information to DB for Demo purposes.
            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAsync(scope.ServiceProvider);
            }

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

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }

        private static string GetConnectionString(IHostApplicationBuilder builder)
        {
            string? connectionString = builder.Configuration.GetConnectionString("SqlServerDev");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Default connection string is not configured!");
            }

            return connectionString;
        }
    }
}
