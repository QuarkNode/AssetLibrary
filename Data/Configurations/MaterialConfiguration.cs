
namespace PartManagementSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using PartManagementSystem.Data.Models;
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public static List<Material> Materials = new()
        {
            new Material
            {
                MaterialId = 1,
                MaterialName = "ABS (Plastic)"
            },
            new Material
            {
                MaterialId = 2,
                MaterialName = "PLA (Plastic)"
            },
            new Material
            {
                MaterialId = 3,
                MaterialName = "PETG (Plastic)"
            },
            new Material
            {
                MaterialId = 4,
                MaterialName = "Nylon (PA12)"
            },
        };

        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasData(Materials);
        }
    }
}
