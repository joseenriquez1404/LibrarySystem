using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetAll()
        {
            var items = await categoryService.GetAllAsync();

            return Ok(items.Select(ToResponse).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var item = await categoryService.GetByIdAsync(id);

            return Ok(ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
        {
            var item = await categoryService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
        {
            var item = await categoryService.UpdateAsync(id, request);

            return Ok(ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await categoryService.DeleteAsync(id);

            return NoContent();
        }

        private static CategoryResponse ToResponse(Category item) => new()
        {
            Id = item.Id,
            Name = item.Name
        };
    }
}
