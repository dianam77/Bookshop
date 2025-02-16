using DataAccess.Data.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Middlewares;
public static class InfraMiddlewareExtension
{
    public static void DbContextInitializer(this IServiceProvider service)
    {
        using (var scope = service.CreateScope())
        {
            var initialiser = scope.ServiceProvider.GetRequiredService<BookDbContextInitializer>();
            initialiser.Initialize();
            initialiser.Seed();
        }
    }
}
