using Sieve.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Bookstore_Management__PROJECT1.Models.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [Sieve(CanFilter = true, CanSort = true)] public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Sieve(CanFilter = true, CanSort = true)]
        public string Email { get; set; }
    }


}
