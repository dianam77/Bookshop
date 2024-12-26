using Core.AuthorService;
using Core.BookService;
using Core.OrderService;
using Core.ServiceFile;
using DataAccess.Data;
using DataAccess.Models;
using DataAccess.Repositories.AuthorRepo;
using DataAccess.Repositories.BasketRepo;
using DataAccess.Repositories.BookRepo;
using DataAccess.Repositories.CommentRepo;
using DataAccess.Repositories.RateBookRepo;
using Microsoft.AspNetCore.Identity;

namespace Bookshop;
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<AuthorService>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<BookService>();
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.AddScoped<OrderService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IRateBookRepository, RateBookRepository>();

        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 0;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<BookDbContext>()
        .AddSignInManager<SignInManager<User>>()
        .AddDefaultTokenProviders();

        return services;
    }
}