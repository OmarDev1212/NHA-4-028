using Microsoft.Extensions.DependencyInjection;
using Sakan.Application.Services;
using Sakan.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IAttachmentService, AttachmentService>();
            return services;
        }
    }
}
