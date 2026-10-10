using PartManagementSystem.Services;
using PartManagementSystem.Services.Contracts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAssetService, AssetService>();

            return services;
        }
    }
}
