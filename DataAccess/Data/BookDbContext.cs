using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class BookDbContext : IdentityDbContext<User, Role, string>
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItems> BasketItems { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RateBookModel> RateBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring precision for AverageRating
            modelBuilder.Entity<Book>()
                .Property(b => b.AverageRating)
                .HasPrecision(18, 2);

            // Additional configurations if necessary
        }
    }
}
