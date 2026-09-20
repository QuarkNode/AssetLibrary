
namespace PartManagementSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using PartManagementSystem.Data.Models;
    using PartManagementSystem.Data.Models.Enum;
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public static List<Project> Projects = new()
        {
            new Project()
            {
                ProjectId = 1,
                ProjectName = "Drone Concept Design - ZEISS",
                OwnerId = 1,
                CreatedOn = new DateTime(2026, 9, 15, 10, 30, 0),
                UpdatedOn = null,
                Status = Status.Draft,
                Description = "A new project about designing a drone for ZEISS."
            },
        };

        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasData(Projects);
        }
    }
}
