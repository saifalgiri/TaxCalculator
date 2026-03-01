using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Context;

namespace TaxCalculator.Tests;

public static class DbContextFactory
{
    public static TaxDbContext Create(string dbName = "")
    {
        var name = string.IsNullOrEmpty(dbName) ? Guid.NewGuid().ToString() : dbName;
        var options = new DbContextOptionsBuilder<TaxDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        var ctx = new TaxDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
