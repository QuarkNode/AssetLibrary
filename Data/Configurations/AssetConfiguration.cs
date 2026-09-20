
namespace PartManagementSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PartManagementSystem.Data.Models;
    using PartManagementSystem.Data.Models.Enum;

    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public static List<Asset> Assets { get; } = new()
        {
            new Asset
            {
                AssetId = 1,
                ProjectId = 1,
                PartNumber = "PN-1001",
                AssetName = "Drone Frame - Main Body",
                Description = "Central chassis for quadcopter frame, mounts motors and battery tray",
                MaterialId = 4,
                Status = Status.Draft,
                CurrentRevisionId = null,
                OwnerId = 1,
                CheckedOutByUserId = null,
                CheckedOutOn = null,
                CreatedOn = new DateTime(2026, 9, 15, 10, 30, 0),
                UpdatedOn = null,
                DeletedOn = null,
                IsDeleted = false
            },
            new Asset
            {
                AssetId = 2,
                ProjectId = 1,
                PartNumber = "PN-1002",
                AssetName = "Motor Mount",
                Description = "Bracket securing brushless motor to the frame arm",
                MaterialId = 2,
                Status = Status.InReview,
                CurrentRevisionId = null,
                OwnerId = 1,
                CheckedOutByUserId = null,
                CheckedOutOn = null,
                CreatedOn = new DateTime(2026, 9, 16, 9, 15, 0),
                UpdatedOn = null,
                DeletedOn = null,
                IsDeleted = false
            }
        };

        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasData(Assets);
        }
    }
}
