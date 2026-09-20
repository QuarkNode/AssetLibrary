
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
                MaterialName = "ABS (Plastic)",
                Density = 1.04m
            },
            new Material
            {
                MaterialId = 2,
                MaterialName = "PLA (Plastic)",
                Density = 1.24m
            },
            new Material
            {
                MaterialId = 3,
                MaterialName = "PETG (Plastic)",
                Density = 1.27m
            },
            new Material
            {
                MaterialId = 4,
                MaterialName = "Nylon (PA12)",
                Density = 1.01m
            },
        };

        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasData(Materials);
        }
    }
}
