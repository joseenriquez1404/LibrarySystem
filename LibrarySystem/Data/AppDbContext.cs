using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Copy> Copies => Set<Copy>();
        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                entity.Property(u => u.Name).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.Property(u => u.PasswordHash).HasMaxLength(255);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Author>(entity =>
            {
                entity.Property(a => a.Name).HasMaxLength(150);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasIndex(b => b.ISBN).IsUnique();
                entity.Property(b => b.ISBN).HasMaxLength(13);
                entity.Property(b => b.Title).HasMaxLength(255);
                entity.Property(b => b.Description).HasMaxLength(2000);

                entity.HasOne(b => b.Category)
                      .WithMany()
                      .HasForeignKey(b => b.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(b => b.Authors)
                      .WithMany();
            });

            modelBuilder.Entity<Copy>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();
                entity.Property(c => c.Code).HasMaxLength(50);
                entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne<Book>()
                      .WithMany()
                      .HasForeignKey(c => c.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Copy>()
                      .WithMany()
                      .HasForeignKey(l => l.CopyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
