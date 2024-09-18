using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Hier voeg je je connection string toe
        optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=Ryancool123;Database=testbase");

        return new AppDbContext(optionsBuilder.Options);
    }
}