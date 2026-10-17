using LibrarySystem.Dtos;
using LibrarySystem.Models;

namespace LibrarySystem.Service
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAllAsync();
        Task<Author> GetByIdAsync(int id);
        Task<Author> CreateAsync(AuthorRequest request);
        Task<Author> UpdateAsync(int id, AuthorRequest request);
        Task DeleteAsync(int id);
    }
}
