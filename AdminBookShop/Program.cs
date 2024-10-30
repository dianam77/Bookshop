using Core.AuthorService;
using Core.BookService;
using Core.FileUpload;
using Core.OrderService;
using DataAccess.Data;
using DataAccess.Models;
using DataAccess.Repositories.AuthorRepo;
using DataAccess.Repositories.BasketRepo;
using DataAccess.Repositories.BookRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BookDbContext>(Options =>
               Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<AuthorService>();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<BookService>();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();

builder.Services.AddIdentity<User, Role>(options =>
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



}).AddEntityFrameworkStores<BookDbContext>()
.AddSignInManager<SignInManager<User>>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
