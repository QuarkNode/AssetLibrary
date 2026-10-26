using AssetLibrary.Data;
using AssetLibrary.Data.Models;
using AssetLibrary.Data.Models.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetLibrary.Web.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AssetLibraryDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

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

        string root = env.WebRootPath;


        var seedDir = Path.Combine(env.ContentRootPath, "SeedData", "Models");
        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "assets");
        Directory.CreateDirectory(uploadsDir);

        var material = await context.Materials.ToListAsync();
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
        }

    }
}