using Bookstore_Management__PROJECT1.Data;
using Bookstore_Management__PROJECT1.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sieve.Services;
using Sieve.Models;
using Sieve.Attributes;
using System;
using System.Diagnostics;
using static System.Reflection.Metadata.BlobBuilder;
using Microsoft.EntityFrameworkCore.Query;
using Bookstore_Management__PROJECT1.Models.DTOs;
using System.ComponentModel.DataAnnotations;



namespace Bookstore_Management__PROJECT1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ILogger<BookController> _logger;
        private readonly BookstoreDbContext bookdbcontext;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SieveProcessor sieveProcessor;
        public BookController(BookstoreDbContext bookdbcontext, SieveProcessor sieveProcessor, ILogger<BookController> logger)
        {
            this.bookdbcontext = bookdbcontext;
            _logger = logger;
            this.sieveProcessor = sieveProcessor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] SieveModel sieveModel, int page = 1, int pageSize = 10)
        {
            try
            {
                // Define initial query without including related entities
                var booksQuery = bookdbcontext.Book.AsQueryable();

                // Apply filtering using Sieve.NET if sieveModel is provided
                if (sieveModel != null)
                {
                    booksQuery = sieveProcessor.Apply(sieveModel, booksQuery);
                }

                // Include related entity (Author) after applying filters
                booksQuery = booksQuery.Include(b => b.Author);

                // Calculate skip count based on pagination parameters
                var skipCount = (page - 1) * pageSize;

                // Apply pagination
                var books = await booksQuery.Skip(skipCount).Take(pageSize).ToListAsync();

                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving books: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            try
            {
                // Retrieve the book by its ID, including the associated Author
                var book = await bookdbcontext.Book
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book == null)
                {
                    // If no book found with the given ID, return HTTP status 404 Not Found
                    return NotFound();
                }

                // Return the book as ActionResult with HTTP status 200 OK
                return Ok(book);
            }
            catch (Exception ex)
            {
                // Log the error message
                _logger.LogError($"Error retrieving book with ID {id}: {ex.Message}");

                // Return HTTP status 500 Internal Server Error with a generic error message
                return StatusCode(500, "Internal server error");
            }
        }
        //---------------------------------------------------------

        [HttpPost]
        public async Task<ActionResult<BookDTO>> PostBook(BookDTO bookDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Return detailed validation errors
                }

                var book = new Book
                {
                    Title = bookDTO.Title,
                    Genre = bookDTO.Genre,
                    PublicationDate = bookDTO.PublicationDate,
                    AuthorId = bookDTO.AuthorId
                };

                bookdbcontext.Book.Add(book);
                await bookdbcontext.SaveChangesAsync();

                var createdBookDTO = new BookDTO
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Genre = book.Genre,
                    PublicationDate = book.PublicationDate,
                    AuthorId = book.AuthorId
                };

                return CreatedAtAction(nameof(GetBookById), new { id = createdBookDTO.BookId }, createdBookDTO);
            }
            catch (Exception ex)
            {
                // Log the error including inner exception details
                _logger.LogError($"Internal server error: {ex.Message} - Inner Exception: {ex.InnerException?.Message}");

                // Return a 500 Internal Server Error with a generic error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        //------------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDTO updatedBookDTO)
        {
            try
            {
                // Ensure the id in the URL matches the id in the updated book DTO
                if (id != updatedBookDTO.BookId)
                {
                    return BadRequest("Book ID mismatch");
                }

                // Check if the book exists
                var existingBook = await bookdbcontext.Book.FindAsync(id);
                if (existingBook == null)
                {
                    return NotFound(); // Return 404 Not Found if book not found
                }

                // Update properties of the existing book entity from DTO
                existingBook.Title = updatedBookDTO.Title;
                existingBook.Genre = updatedBookDTO.Genre;
                existingBook.PublicationDate = updatedBookDTO.PublicationDate;

                // If updating the AuthorId, ensure Author exists in database and assign
                if (updatedBookDTO.AuthorId != existingBook.AuthorId)
                {
                    var author = await bookdbcontext.Author.FindAsync(updatedBookDTO.AuthorId);
                    if (author == null)
                    {
                        return NotFound("Author not found");
                    }
                    existingBook.AuthorId = updatedBookDTO.AuthorId;
                }

                // Validate the updated book entity
                var validationContext = new ValidationContext(existingBook, null, null);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(existingBook, validationContext, validationResults, true);
                if (!isValid)
                {
                    foreach (var validationResult in validationResults)
                    {
                        ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                    }
                    return BadRequest(ModelState); // Return 400 Bad Request with validation errors
                }

                // Save changes to database
                bookdbcontext.Entry(existingBook).State = EntityState.Modified;
                await bookdbcontext.SaveChangesAsync();

                return NoContent(); // Return 204 No Content on success
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError($"Error updating book: {ex.Message}");

                return StatusCode(500, "Internal server error"); // Return 500 Internal Server Error on exception
            }
        }


        //-----------------------------------------------------------------------------------

        [HttpDelete("{id}")]
        [ProducesResponseType(204)] // No Content (Success)
        [ProducesResponseType(200)]
        [ProducesResponseType(400)] // Bad Request
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await bookdbcontext.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound(); // 404 if book not found
            }

            bookdbcontext.Book.Remove(book);
            await bookdbcontext.SaveChangesAsync();

            return NoContent(); // 204 (Success, no content)
        }


    }
}











