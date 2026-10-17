using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class CategoryService(AppDbContext _context) : ICategoryService
    {
        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await FindAsync(id);
        }

        public async Task<Category> CreateAsync(CategoryRequest request)
        {
            var name = request.Name.Trim();

            await EnsureNameIsFreeAsync(name, null);

            var category = new Category { Name = name };
            _context.Categories.Add(category);

            await SaveAsync();

            return category;
        }

        public async Task<Category> UpdateAsync(int id, CategoryRequest request)
        {
            var category = await FindAsync(id);
            var name = request.Name.Trim();

            await EnsureNameIsFreeAsync(name, id);

            category.Name = name;

            await SaveAsync();

            return category;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await FindAsync(id);

            if (await _context.Books.AnyAsync(b => b.CategoryId == id))
            {
                throw new ConflictException("The category has books and cannot be deleted.");
            }

            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
            {
                // Se agregó un libro a la categoría entre la verificación y el borrado.
                throw new ConflictException("The category has books and cannot be deleted.");
            }
        }

        private async Task<Category> FindAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            return category ?? throw new NotFoundException("Category not found.");
        }

        // Con el collation por defecto de SQL Server la comparación no distingue mayúsculas, igual que el índice único.
        private async Task EnsureNameIsFreeAsync(string name, int? excludeId)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId))
            {
                throw new ConflictException("A category with that name already exists.");
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // Dos peticiones simultáneas con el mismo nombre: gana el índice único.
                throw new ConflictException("A category with that name already exists.");
            }
        }
    }
}
