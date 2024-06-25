using Bookstore_Management__PROJECT1.Data;
using Bookstore_Management__PROJECT1.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookstore_Management__PROJECT1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly ILogger<AuthorController> _logger; // Define ILogger
        private readonly BookstoreDbContext authordbcontext;

        public AuthorController(BookstoreDbContext authordbcontext, ILogger<AuthorController> logger)
        {
            this.authordbcontext = authordbcontext;
            _logger = logger;
        }


        [HttpGet]
        // Specify the API version here
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            return await authordbcontext.Author.Include(a => a.Books).ToListAsync();
        }


        // GET /api/authors/{id} - Get author by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await authordbcontext.Author.Include(a => a.Books).FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            return author;
        }

       [HttpPost]
        public async Task<ActionResult<Author>> PostAuthor(AuthorDTO authorDTO)
        {
            if (authorDTO == null || authorDTO.Books == null)
            {
                return BadRequest("Invalid data received.");
            }

            try
            {
                var author = new Author
                {
                    Name = authorDTO.Name,
                    Books = new List<Book>()
                };

                foreach (var bookDTO in authorDTO.Books)
                {
                    var book = new Book
                    {
                        Title = bookDTO.Title,
                        Genre = bookDTO.Genre,
                        PublicationDate = bookDTO.PublicationDate,
                        Author = author  // Associate book with the author
                    };
                    author.Books.Add(book);
                }

                authordbcontext.Author.Add(author);
                await authordbcontext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, author);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //---------------------------------------------------------------------------------------

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorDTO updatedAuthorDTO)
        {
            try
            {
                if (id != updatedAuthorDTO.AuthorId)
                {
                    return BadRequest("Author ID mismatch");
                }

                var existingAuthor = await authordbcontext.Author.FindAsync(id);

                if (existingAuthor == null)
                {
                    return NotFound("Author not found");
                }

                existingAuthor.Name = updatedAuthorDTO.Name;
                // Optionally update other properties like Books here

                // Validate the updated author entity
                var validationContext = new ValidationContext(existingAuthor, null, null);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(existingAuthor, validationContext, validationResults, true);
                if (!isValid)
                {
                    foreach (var validationResult in validationResults)
                    {
                        ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                    }
                    return BadRequest(ModelState); // Return 400 Bad Request with validation errors
                }

                authordbcontext.Entry(existingAuthor).State = EntityState.Modified;
                await authordbcontext.SaveChangesAsync();

                return NoContent(); // Return 204 No Content on success
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating author: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        //----------------------------------------------------------------------------------
        // DELETE /api/authors/{id} - Delete an author
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await authordbcontext.Author.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            authordbcontext.Author.Remove(author);
            await authordbcontext.SaveChangesAsync();
            return NoContent();
        }

    }
}
       