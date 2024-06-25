using Bookstore_Management__PROJECT1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookstore_Management__PROJECT1.Data
{
    public class BookstoreDbContext : DbContext
    {
        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options) : base(options)

        {

        }
        public DbSet<Book> Book { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<Customer> Customer { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Book entity
            modelBuilder.Entity<Book>()
                .HasKey(b => b.BookId);

            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Genre);

            modelBuilder.Entity<Book>()

                .Property(b => b.PublicationDate)
                .HasColumnType("date")
                .IsRequired();

            // Configure Book-Author relationship
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId);

            

            //--------------------------------------------------------------------------------------------------
            modelBuilder.Entity<Author>()
            .HasKey(a => a.AuthorId); // Configure primary key

            modelBuilder.Entity<Author>()
                .Property(a => a.Name);
                

            // Configure Author-Book relationship
            modelBuilder.Entity<Author>()
                .HasMany(a => a.Books)     // Author has many Books
                .WithOne(b => b.Author)    // Book has one Author
                .HasForeignKey(b => b.AuthorId); // Foreign key in Book entity
                                                 //-------------------------------------------------------------------------------
                                                 // Fluent API configurations
        }
    }
}

   