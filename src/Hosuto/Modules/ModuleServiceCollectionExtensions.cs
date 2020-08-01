using System;
using System.Threading;
using System.Threading.Tasks;
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
            services.AddSingleton<THostedService>();
            return services.AddSingleton<IHostedService, ModuleHostedService<THostedService>>();
        }

        public static IServiceCollection AddModuleHandler<TModuleHandler>(
            this IServiceCollection services)
            where TModuleHandler : class, IHostedServiceHandler
        {
            return services.AddSingleton<IHostedService, ModuleHandlerHostService<TModuleHandler>>();
        }

        public static IServiceCollection AddModuleHandler(
            this IServiceCollection services,
            Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            return services.AddSingleton<IHostedService>(sp =>
                new ModuleHandlerHostService<DelegateServiceHandler>(sp, new DelegateServiceHandler(handlerDelegate)));
        }

    }
}