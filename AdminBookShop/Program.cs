using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using AdminBookShop;
using DataAccess.Middlewares;
using DataAccess.Data.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Retrieve and validate the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured properly.");
}

// Log connection string for debugging (remove in production)
Console.WriteLine($"Using Connection String: {connectionString}");

// Register services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Register Database Context
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<BookDbContextInitializer>();
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

// Initialize and Seed Database (only in Development)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbInitializer = services.GetRequiredService<BookDbContextInitializer>();

    try
    {
        dbInitializer.Initialize();
        dbInitializer.Seed();
        Console.WriteLine("Database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
    }
}

// Ensure upload directory exists
string uploadPath = builder.Configuration["FileUpload:StoragePath"] ?? "wwwroot/Uploads";
Directory.CreateDirectory(uploadPath);

// Configure the HTTP request pipeline
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
