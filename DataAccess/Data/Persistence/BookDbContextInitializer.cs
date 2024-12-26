using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Data.Persistence;
public class BookDbContextInitializer
{
    private readonly ILogger<BookDbContextInitializer> _logger;
    private readonly BookDbContext _context;
    private readonly UserManager<User> _userManager;

    public BookDbContextInitializer(ILogger<BookDbContextInitializer> logger, BookDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public void Initialize()
    {
        try
        {
            _context.Database.Migrate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public void Seed()
    {
        try
        {
            TrySeed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public void TrySeed()
    {
        SeedUsers(_userManager).Wait();

        if (!_context.Authors.Any())
        {
            _context.Authors.AddRange(
                new Author { Name = "اریش ماریا رمارک" },
                new Author { Name = "کریستوفر پائولینی" }
            );
        }

        if (!_context.Books.Any())
        {
            _context.Books.AddRange(
                new Book
                {
                    Title = "به وقت عشق و مرگ",
                    Description = "اریش ماریا رمارک، نویسنده‌ی شهیر آلمانی در اثر داستانی خود کتاب به وقت عشق و مرگ، روشنایی زندگی و تاریکی مرگ را به مصاف هم درمی‌آورد. قهرمان و راوی این رمان تاریخی یک سرباز به نام ارنست گریبر است که پس از ماه‌ها خدمت در جبهه‌های جنگ حالا فرصت کوتاهی پیدا می‌کند تا به خانه و زندگی‌اش سری بزند. اما با ورود ارنست به شهر و دیار خود، مرد جوان با حقایقی رو‌به‌رو می‌شود که به‌سختی قابل باور کردن است.",
                    Price = 100000,
                    Img = "/uploads/test1.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = false,
                    AuthorId = 1
                },
                new Book
                {
                    Title = "الدست",
                    Description = "داستان‌های فانتزی و هیجان انگیز...",
                    Price = 460000,
                    Img = "/uploads/test2.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = true,
                    AuthorId = 2
                },
                new Book
                {
                    Title = "من پیام رسان هستم",
                    Description = "کتاب من پیام رسان هستم نوشته‌ی مارکوس زوساک، داستان زندگی راننده‌تاکسی جوان و ناامید و تنهایی است که ناخواسته وارد یک بازی پیچیده می‌شود و مأموریت‌هایی عجیب به او محول می‌گردد. اد کِندی پس‌ از اینکه به شکلی تصادفی مانع سرقت از یک بانک می‌شود و به شهرت می‌رسد، نامه‌ای ناشناس در صندوق پستی خود می‌یابد... گفتنی است که این کتاب برنده‌ی جایزه‌ی شورای کتاب کودک استرالیا شده است.",
                    Price = 58000,
                    Img = "/uploads/test3.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = true,
                    AuthorId = 2
                }
            );
        }

        // Adding Comments and Replies
        if (!_context.Comments.Any())
        {
            var comments = new List<Comment>
        {
            new Comment
            {
                Text = "این کتاب فوق‌العاده است!",
                BookId = 1,
                UserId = "user1",
                UserName = "User A",
                Created = DateTime.Now
            },
            new Comment
            {
                Text = "چقدر جذاب!",
                BookId = 1,
                UserId = "user2",
                UserName = "User B",
                Created = DateTime.Now
            }
        };

            // Add Comments to Context
            _context.Comments.AddRange(comments);
            _context.SaveChanges();

            // Adding Replies
            var replies = new List<Comment>
        {
            new Comment
            {
                Text = "کاملاً موافقم!",
                BookId = 1,
                UserId = "admin",
                UserName = "Admin",
                Created = DateTime.Now,
                ReplyId = comments[0].Id // Reply to the first comment
            },
            new Comment
            {
                Text = "ممنون بابت بازخوردتون!",
                BookId = 1,
                UserId = "admin",
                UserName = "Admin",
                Created = DateTime.Now,
                ReplyId = comments[1].Id // Reply to the second comment
            }
        };

            // Add Replies to Context
            _context.Comments.AddRange(replies);
        }

        _context.SaveChanges();
    }


    private static async Task SeedUsers(UserManager<User> userManager)
    {
        // Define the users to be added
        var users = new List<User>
    {
        new User
        {
            Id = "user1",
            UserName = "UserA",
            NormalizedUserName = "USERA",
            Email = "usera@example.com",
            NormalizedEmail = "USERA@EXAMPLE.COM",
            FullName = "User A",
            SecurityStamp = Guid.NewGuid().ToString()
        },
        new User
        {
            Id = "user2",
            UserName = "UserB",
            NormalizedUserName = "USERB",
            Email = "userb@example.com",
            NormalizedEmail = "USERB@EXAMPLE.COM",
            FullName = "User B",
            SecurityStamp = Guid.NewGuid().ToString()
        }
    };

        // Define the passwords for the users
        var userPasswords = new Dictionary<string, string>
    {
        { "UserA", "zxcvb1234" },
        { "UserB", "asdfg1234" }
    };

        foreach (var user in users)
        {
            var existingUser = await userManager.FindByNameAsync(user.UserName);
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(user, userPasswords[user.UserName]);
                if (result.Succeeded)
                {
                    Console.WriteLine($"User {user.UserName} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Error creating user {user.UserName}:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"User {user.UserName} already exists.");
            }
        }

        // Check if the admin user already exists
        var adminName = "Admin";
        var adminEmail = "admin@example.com";
        var adminPassword = "admin1234";

        var admin = await userManager.FindByNameAsync(adminName);
        if (admin == null)
        {
            admin = new User
            {
                Id = "admin",
                UserName = adminName,
                NormalizedUserName = adminName.ToUpper(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                FullName = "Admin User",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                Console.WriteLine("Admin created successfully.");
            }
            else
            {
                Console.WriteLine("Error creating admin:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("Admin already exists.");
        }
    }

}
