using Sieve.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Bookstore_Management__PROJECT1.Models.Entities
{
    using Sieve.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;
    public class Book
    {
        public int BookId { get; set; }


        [Sieve(CanFilter = true, CanSort = true)] public string Title { get; set; }

        [Sieve(CanFilter = true, CanSort = true)] public string Genre { get; set; }

        [Required][Sieve(CanFilter = true, CanSort = true)] public DateTime PublicationDate { get; set; }

        
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
