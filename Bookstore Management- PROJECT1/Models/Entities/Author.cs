using System.Text.Json.Serialization;

namespace Bookstore_Management__PROJECT1.Models.Entities
{
    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        [JsonIgnore] // Ignore serialization of Books to prevent cycles
        public ICollection<Book> Books { get; set; }

    }
}
