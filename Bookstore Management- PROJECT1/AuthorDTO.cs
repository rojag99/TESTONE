using Bookstore_Management__PROJECT1.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Bookstore_Management__PROJECT1
{
    public class AuthorDTO
    {
        [Required(ErrorMessage = "Author Id is required.")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public ICollection<BookDTO> Books { get; set; }
    }
}
