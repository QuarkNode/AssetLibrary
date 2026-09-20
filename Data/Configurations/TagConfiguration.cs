
namespace PartManagementSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PartManagementSystem.Data.Models;
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public static List<Tag> Tags = new()
        {
            new Tag
            {
                TagId = 1,
                TagName = "Ready",
                IsCustomTag = false
            },
            new Tag
            {
                TagId = 2,
                TagName = "Work In Progress",
                IsCustomTag = false
            },
            new Tag
            {
                TagId = 3,
                TagName = "Cancelled",
                IsCustomTag = false
            }
        };

        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasData(Tags);
        }
    }
}
