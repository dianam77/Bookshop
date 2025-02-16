using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using AdminBookShop;
using DataAccess.Middlewares;
using DataAccess.Data.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Register DbContext with SQL Server
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ensure the database is initialized properly
builder.Services.AddScoped<BookDbContextInitializer>();

// Register your services (repositories, services, etc.)
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

// Initialize and Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbInitializer = services.GetRequiredService<BookDbContextInitializer>();
    dbInitializer.Initialize();
    dbInitializer.Seed();
}

// Create the upload directory (if needed)
string uploadPath = builder.Configuration["FilePaths:UploadPath"] ?? "wwwroot/Uploads";
Directory.CreateDirectory(uploadPath);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
