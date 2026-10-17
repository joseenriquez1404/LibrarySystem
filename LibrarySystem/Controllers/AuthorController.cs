using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/authors")]
    [Authorize]
    public class AuthorController(IAuthorService authorService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuthorResponse>>> GetAll()
        {
            var items = await authorService.GetAllAsync();

            return Ok(items.Select(ToResponse).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AuthorResponse>> GetById(int id)
        {
            var item = await authorService.GetByIdAsync(id);

            return Ok(ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<AuthorResponse>> Create(AuthorRequest request)
        {
            var item = await authorService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AuthorResponse>> Update(int id, AuthorRequest request)
        {
            var item = await authorService.UpdateAsync(id, request);

            return Ok(ToResponse(item));
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await authorService.DeleteAsync(id);

            return NoContent();
        }

        private static AuthorResponse ToResponse(Author item) => new()
        {
            Id = item.Id,
            Name = item.Name
        };
    }
}
