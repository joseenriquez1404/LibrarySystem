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
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Fine> Fines => Set<Fine>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasIndex(r => new { r.BookId, r.Status, r.Position });
                entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Book>()
                      .WithMany()
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Fine>(entity =>
            {
                entity.Property(f => f.Amount).HasPrecision(10, 2);
                entity.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne<Loan>()
                      .WithMany()
                      .HasForeignKey(f => f.LoanId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.Date);
                entity.Property(a => a.Action).HasMaxLength(100);
                entity.Property(a => a.Entity).HasMaxLength(100);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(a => a.ActorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
