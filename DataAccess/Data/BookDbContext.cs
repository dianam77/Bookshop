using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class BookDbContext : IdentityDbContext<User, Role, string>
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItems> BasketItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Authors
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, Name = "Author A" },
                new Author { Id = 2, Name = "Author B" }
            );

            // Seed Books
            modelBuilder.Entity<Book>().HasData(
                    new Book
                    {
                        Id = 1,
                        Title = "Book A",
                        Description = "Description A",
                        Price = 10,
                        Img = Path.Combine("wwwroot", "uploads", "Screenshot 2024-10-15 125514.jpg"),
                        Created = DateTime.Now,
                        IsAvail = true,
                        ShowHomePage = true,
                        AuthorId = 1
                    },
                    new Book
                    {
                        Id = 2,
                        Title = "Book B",
                        Description = "Description B",
                        Price = 15,
                        Img = Path.Combine("wwwroot", "uploads", "Screenshot 2024-10-15 125514.jpg"), 
                        Created = DateTime.Now,
                        IsAvail = false,
                        ShowHomePage = false,
                        AuthorId = 2
                    }
                );
            // Seed Baskets
            modelBuilder.Entity<Basket>().HasData(
                new Basket { Id = 1, Created = DateTime.Now, Payed = DateTime.Now.AddDays(1), UserId = "user1", Address = "123 Street", Mobile = "123456789", Status = Status.Pending }
            );

            // Seed Basket Items
            modelBuilder.Entity<BasketItems>().HasData(
                new BasketItems { Id = 1, BasketId = 1, BookId = 1, Price = 10, Qty = 1, Created = DateTime.Now, Status = Status.Pending }
            );

            // Seed Users with Hashed Passwords
            var hasher = new PasswordHasher<User>();
            var user1 = new User
            {
                Id = "user1",
                UserName = "UserA",
                NormalizedUserName = "USERA",
                Email = "usera@example.com",
                NormalizedEmail = "USERA@EXAMPLE.COM",
                FullName = "User A",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Hash the password and assign it to the user
            user1.PasswordHash = hasher.HashPassword(user1, "Password123!");

            modelBuilder.Entity<User>().HasData(user1);
        }
    }
}