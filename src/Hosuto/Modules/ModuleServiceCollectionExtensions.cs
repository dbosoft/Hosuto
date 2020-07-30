using Dbosoft.Hosuto.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public static class ModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleService<THostedService>(
            this IServiceCollection services)
            where THostedService : class, IHostedService
        {
            return services.AddSingleton<IHostedService, ModuleHostedService<THostedService>>();
        }

        public static IServiceCollection AddModuleHandler<TModuleHandler>(
            this IServiceCollection services)
            where TModuleHandler : class, IHostedServiceHandler
        {
            return services.AddSingleton<IHostedService, ModuleHostedHandler<TModuleHandler>>();
        }


    }
}