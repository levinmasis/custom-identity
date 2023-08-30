using custom_identity.Data.Context;
using custom_identity.Data.Entities;
using custom_identity.Infrastructure.Messaging;
using custom_identity.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace custom_identity.Extensions
{
    public static class Dependencies
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CustomIdentityDbConnection");

            services.AddDbContext<CustomIdentityDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<User, IdentityRole<long>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<CustomIdentityDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddInfrastructureDepenencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmailSender, EmailSender>();

            return services;
        }
    }
}
