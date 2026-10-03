using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using PartManagementSystem.Data.Models;

namespace PartManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        /* This is where we can configure our DB */
        /* Add Entities, FLUENT API, and more ...*/
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public virtual DbSet<Asset> Assets { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<Project> Projects { get; set; } = null!;
        public virtual DbSet<Revision> Revisions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* Configure Fluent API for Entities here. */

            modelBuilder.Entity<Revision>()
                .HasIndex(r => new { r.AssetId, r.RevisionNumber })
                .IsUnique();

            modelBuilder.Entity<Revision>()
                .HasOne(r => r.Asset)
                .WithMany(a => a.Revisions)
                .HasForeignKey(r => r.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Revision>()
                .HasOne(r => r.UploadedByUser)
                .WithMany()
                .HasForeignKey(r => r.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Owner)
                .WithMany()
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Assets)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Material)
                .WithMany()
                .HasForeignKey(a => a.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.CurrentRevision)
                .WithMany()
                .HasForeignKey(a => a.CurrentRevisionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);


            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
