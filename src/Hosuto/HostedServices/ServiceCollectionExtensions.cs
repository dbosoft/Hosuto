using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.HostedServices
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddHostedHandler<TServiceHandler>(
            this IServiceCollection services)
            where TServiceHandler : class, IHostedServiceHandler
        {
            services.AddSingleton<TServiceHandler>();
            return services.AddSingleton<IHostedService,HandlerHostService<TServiceHandler>>();
        }

        public static IServiceCollection AddHostedHandler(
            this IServiceCollection services, 
            Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            return services.AddSingleton<IHostedService>(sp =>
                new HandlerHostService<DelegateServiceHandler>(sp,new DelegateServiceHandler(handlerDelegate)));
        }

    }
}