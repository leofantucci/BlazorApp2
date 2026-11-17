using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Repositorio;

public sealed class MyDBContextFactory : IDesignTimeDbContextFactory<MyDBContext>
{
    public MyDBContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Defina DATABASE_URL antes de executar as ferramentas do Entity Framework.");

        var options = new DbContextOptionsBuilder<MyDBContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MyDBContext(options);
    }
}
