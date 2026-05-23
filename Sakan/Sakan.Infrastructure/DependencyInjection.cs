using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Infrastructure.Data;
using Sakan.Infrastructure.Repositories;

namespace Sakan.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
            });


            services.AddScoped<IUnitOfWork, UnitOfWork>();

            


            return services;
        }
    }
}
