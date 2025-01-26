using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using AdminBookShop;
using DataAccess.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

string uploadPath = builder.Configuration["FilePaths:UploadPath"]?? "wwwroot/Uploads";
Directory.CreateDirectory(uploadPath);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Services.DbContextInitializer();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
