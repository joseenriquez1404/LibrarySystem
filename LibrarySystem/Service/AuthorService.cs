using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class AuthorService(AppDbContext _context) : IAuthorService
    {
        public async Task<List<Author>> GetAllAsync()
        {
            return await _context.Authors
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Author> GetByIdAsync(int id)
        {
            return await FindAsync(id);
        }

        public async Task<Author> CreateAsync(AuthorRequest request)
        {
            var author = new Author { Name = request.Name.Trim() };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return author;
        }

        public async Task<Author> UpdateAsync(int id, AuthorRequest request)
        {
            var author = await FindAsync(id);

            author.Name = request.Name.Trim();
            await _context.SaveChangesAsync();

            return author;
        }

        public async Task DeleteAsync(int id)
        {
            var author = await FindAsync(id);

            if (await _context.Books.AnyAsync(b => b.Authors.Any(a => a.Id == id)))
            {
                throw new ConflictException("The author has books and cannot be deleted.");
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        private async Task<Author> FindAsync(int id)
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);

            return author ?? throw new NotFoundException("Author not found.");
        }
    }
}
