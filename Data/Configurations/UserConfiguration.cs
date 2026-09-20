
namespace PartManagementSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using PartManagementSystem.Data.Models;
    using PartManagementSystem.Data.Models.Enum;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public static List<User> Users = new()
        {
            new User
            {
                UserId = 1,
                CompanyName = null,
                FirstName = "Ivan",
                MiddleName = null,
                LastName = "Petrov",
                Age = 24,
                Gender = Gender.Male,
                Email = "ivan.petrov@example.com",
                Occupation = "Design Engineer",
                City = "Plovdiv",
                Country = "Bulgaria",
                CreatedOn = new DateTime(2026, 9, 10, 8, 0, 0),
                DeletedOn = null,
                IsDeleted = false
            }
        };

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(Users);
        }
    }
}
