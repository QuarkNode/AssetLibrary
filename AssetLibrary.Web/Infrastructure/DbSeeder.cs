
using AssetLibrary.Data;
using AssetLibrary.Data.Models;
using AssetLibrary.Data.Models.Enum;
using AssetLibrary.Services.Contracts;
using AssetLibrary.Services.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetLibrary.Web.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, string baseUrl)
    {
        var context = services.GetRequiredService<AssetLibraryDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var thumbnailService = services.GetRequiredService<IThumbnailService>();

        await context.Database.MigrateAsync();

        if (await context.Projects.AnyAsync())
        {
            return;
        }

        var user = await userManager.FindByEmailAsync("user@example.com");

        if (user == null)
        {
            user = new User
            {
                CompanyName = "AssetLibrary Company",
                FirstName = "Demo",
                LastName = "User",
                UserName = "user@example.com",
                DateOfBirth = new DateTime(2000, 01, 01),
                Gender = Gender.Male,
                City = "Plovdiv",
                Country = "Bulgaria",
                CreatedOn = DateTime.UtcNow,
                Email = "user@example.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password123!");

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the demo user: {errors}");
            }
        }

        var project = new Project
        {
            ProjectName = "Dome Project - Mock Presentation",
            Owner = user,
            CreatedOn = DateTime.UtcNow,
            Status = Status.Draft,
            Description = "A mock presentation project for showcasing the dome concept and its related assets."
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var seedDir = Path.Combine(env.ContentRootPath, "SeedData", "Models");
        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "assets");

        Directory.CreateDirectory(uploadsDir);

        var material = await context.Materials.ToListAsync();

        if (material.Count == 0)
        {
            throw new InvalidOperationException("No materials are available for seeding.");
        }

        int number = 0;

        foreach (var file in Directory.GetFiles(seedDir))
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();

            if (extension is not (".stl" or ".obj" or ".fbx"))
            {
                continue;
            }

            number++;

            var storedName = $"{Guid.NewGuid()}{extension}";

            var asset = new Asset
            {
                ProjectId = project.ProjectId,
                AssetName = Path.GetFileNameWithoutExtension(file),
                PartNumber = $"DOME-{number:D3}",
                Description = "Seeded asset.",
                MaterialId = material[0].MaterialId,
                OwnerId = user.Id,
                CreatedOn = DateTime.UtcNow,
                Status = Status.Draft
            };

            context.Assets.Add(asset);
            await context.SaveChangesAsync();

            var assetDir = Path.Combine(uploadsDir, asset.AssetId.ToString());
            Directory.CreateDirectory(assetDir);

            File.Copy(file, Path.Combine(assetDir, storedName));

            var revision = new Revision
            {
                AssetId = asset.AssetId,
                FilePath = $"/uploads/assets/{asset.AssetId}/{storedName}",
                FileExtension = extension,
                FileSizeBytes = new FileInfo(file).Length,
                OriginalFileName = Path.GetFileName(file),
                ChangeNotes = "Initial revision",
                RevisionNumber = 1,
                UploadedByUserId = user.Id,
                UploadedOn = DateTime.UtcNow
            };

            context.Revisions.Add(revision);
            await context.SaveChangesAsync();

            asset.CurrentRevisionId = revision.RevisionId;
            await context.SaveChangesAsync();

            try
            {
                string rendererUrl = $"{baseUrl.TrimEnd('/')}/dist/thumbnail-renderer.html";
                string modelUrl = $"{baseUrl.TrimEnd('/')}{revision.FilePath}";

                byte[] thumbnailBytes = await thumbnailService.GenerateAssetThumbnailAsync(rendererUrl, modelUrl);

                using var thumbnailStream = new MemoryStream(thumbnailBytes);

                var thumbnail = new ThumbnailUpload
                {
                    Content = thumbnailStream,
                    FileName = "thumbnail.png",
                    Length = thumbnailBytes.Length
                };

                await thumbnailService.SaveAssetThumbnailAsync(revision.RevisionId, thumbnail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Thumbnail generation failed for {file}: {ex.Message}");
            }
        }

        try
        {
            string rendererUrl = $"{baseUrl.TrimEnd('/')}/dist/thumbnail-renderer.html";

            var modelUrls = await context.Assets
                .Where(a => a.ProjectId == project.ProjectId)
                .Select(a => a.CurrentRevision!.FilePath)
                .ToListAsync();

            if (modelUrls.Count > 0)
            {
                var absoluteModelUrls = modelUrls
                    .Select(path => $"{baseUrl.TrimEnd('/')}{path}")
                    .ToList();

                byte[] thumbnailBytes = await thumbnailService.GenerateProjectThumbnailAsync(rendererUrl, absoluteModelUrls);

                using var thumbnailStream = new MemoryStream(thumbnailBytes);

                var thumbnail = new ThumbnailUpload
                {
                    Content = thumbnailStream,
                    FileName = "project-thumbnail.png",
                    Length = thumbnailBytes.Length
                };

                await thumbnailService.SaveProjectThumbnailAsync(project.ProjectId, thumbnail);

                project.ThumbnailGeneratedOn = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Project thumbnail generation failed: {ex.Message}");
        }
    }
}
