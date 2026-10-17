using LibrarySystem.Dtos;
using LibrarySystem.Models;

namespace LibrarySystem.Service
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> CreateAsync(CategoryRequest request);
        Task<Category> UpdateAsync(int id, CategoryRequest request);
        Task DeleteAsync(int id);
    }
}
