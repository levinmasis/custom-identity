using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace custom_identity.Data.Context
{
    public class CustomIdentityDbContextFactory : IDesignTimeDbContextFactory<CustomIdentityDbContext>
    {
        public CustomIdentityDbContext CreateDbContext(string[] args)
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string basePath = Directory.GetCurrentDirectory();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CustomIdentityDbContext>();
            var connectionString = configuration.GetConnectionString("CustomIdentityDbConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new CustomIdentityDbContext(optionsBuilder.Options);
        }
    }
}
