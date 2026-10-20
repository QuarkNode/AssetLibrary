
namespace AssetLibrary.Extensions
{
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Services;
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IHomeService, HomeService>();

            return services;
        }
    }
}
