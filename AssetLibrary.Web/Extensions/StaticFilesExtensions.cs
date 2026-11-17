
namespace AssetLibrary.Web.Extensions
{
    using Microsoft.AspNetCore.StaticFiles;
    public static class StaticFilesExtensions
    {
        public static IApplicationBuilder UseCustomStaticFiles(this IApplicationBuilder app)
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            contentTypeProvider.Mappings[".obj"] = "application/octet-stream";
            contentTypeProvider.Mappings[".stl"] = "application/octet-stream";
            contentTypeProvider.Mappings[".fbx"] = "application/octet-stream";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider
            });

            return app;
        }
    }
}
