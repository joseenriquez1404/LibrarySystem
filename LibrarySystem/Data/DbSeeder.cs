using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data
{
    // Datos de ejemplo para desarrollo. Las contraseñas son solo para pruebas locales.
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Users.AnyAsync())
            {
                return;
            }

            var hasher = new PasswordHasher<User>();

            var admin = new User { Name = "Admin", Email = "admin@biblioteca.local", Role = Roles.Admin };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

            var librarian = new User { Name = "Bibliotecario", Email = "bibliotecario@biblioteca.local", Role = Roles.Librarian };
            librarian.PasswordHash = hasher.HashPassword(librarian, "Biblio123!");

            var reader1 = new User { Name = "Lector Uno", Email = "lector1@biblioteca.local", Role = Roles.User };
            reader1.PasswordHash = hasher.HashPassword(reader1, "Lector123!");

            var reader2 = new User { Name = "Lector Dos", Email = "lector2@biblioteca.local", Role = Roles.User };
            reader2.PasswordHash = hasher.HashPassword(reader2, "Lector123!");

            db.Users.AddRange(admin, librarian, reader1, reader2);

            var novela = new Category { Name = "Novela" };
            var ciencia = new Category { Name = "Ciencia" };
            var historia = new Category { Name = "Historia" };
            db.Categories.AddRange(novela, ciencia, historia);

            var garcia = new Author { Name = "Gabriel García Márquez" };
            var asimov = new Author { Name = "Isaac Asimov" };
            var harari = new Author { Name = "Yuval Noah Harari" };
            db.Authors.AddRange(garcia, asimov, harari);

            var cienAnios = new Book
            {
                Title = "Cien años de soledad",
                Description = "Historia de la familia Buendía en Macondo.",
                ISBN = "9780307474728",
                Year = 1967,
                Category = novela,
                Authors = new List<Author> { garcia }
            };

            var fundacion = new Book
            {
                Title = "Fundación",
                Description = "Un matemático intenta preservar el conocimiento de la humanidad.",
                ISBN = "9780553293357",
                Year = 1951,
                Category = ciencia,
                Authors = new List<Author> { asimov }
            };

            var sapiens = new Book
            {
                Title = "Sapiens",
                Description = "Breve historia de la humanidad.",
                ISBN = "9780062316097",
                Year = 2011,
                Category = historia,
                Authors = new List<Author> { harari }
            };

            db.Books.AddRange(cienAnios, fundacion, sapiens);
            await db.SaveChangesAsync();

            db.Copies.AddRange(
                new Copy { BookId = cienAnios.Id, Code = "CAS-001" },
                new Copy { BookId = cienAnios.Id, Code = "CAS-002" },
                new Copy { BookId = fundacion.Id, Code = "FUN-001" },
                new Copy { BookId = sapiens.Id, Code = "SAP-001" },
                new Copy { BookId = sapiens.Id, Code = "SAP-002" }
            );

            await db.SaveChangesAsync();
        }
    }
}
