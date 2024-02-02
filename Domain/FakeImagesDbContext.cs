using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class FakeImagesDbContext : DbContext
    {
        public FakeImagesDbContext(DbContextOptions<FakeImagesDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<FakeImage> FakeImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.email_verified_at)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.author_id)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.name)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.original_photo_url)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.original_back_url)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.upload_at)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.resize_photo_url)
                .IsRequired(false);

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.resize_back_url)
                .IsRequired(false);

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.resized_at)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.no_back_photo_url)
                .IsRequired(false);

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.remove_bg_at)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.result_photo_url)
                .IsRequired(false);

            modelBuilder.Entity<FakeImage>()
                .Property(fi => fi.finish_at)
                .IsRequired();

            modelBuilder.Entity<FakeImage>()
                .HasOne(fi => fi.AuthorId)
                .WithMany(u => u.FakeImages)
                .HasForeignKey(fi => fi.author_id);
        }
    }
}
