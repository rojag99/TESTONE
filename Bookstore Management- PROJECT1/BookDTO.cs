using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore_Management__PROJECT1.Models.DTOs
{
    public class BookDTO
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "Publication date is required.")]
        public DateTime PublicationDate { get; set; }

        [Required(ErrorMessage = "AuthorId is required.")]
        public int AuthorId { get; set; }
    }
}
