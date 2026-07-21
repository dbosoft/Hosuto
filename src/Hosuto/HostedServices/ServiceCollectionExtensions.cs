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
            // Register the handler through a factory instead of by implementation type.
            // Since .NET 9 the built-in container enables ServiceProviderOptions.ValidateOnBuild
            // by default in the Development environment, which eagerly constructs every
            // implementation-type singleton when the (web) module host is built - i.e. before the
            // module container (SimpleInjector/Autofac) is configured later in the bootstrap
            // pipeline. Handlers whose dependencies live in that container therefore fail
            // validation at Build() time. A factory registration is skipped by ValidateOnBuild; at
            // runtime the handler is still activated from the module's (cross-wired) service provider
            // when the hosted service executes. Handlers are expected to have a single public
            // constructor, for which ActivatorUtilities selects the same constructor as before.
            services.AddSingleton(sp => ActivatorUtilities.CreateInstance<TServiceHandler>(sp));
            return services.AddHostedService<HandlerHostService<TServiceHandler>>();
        }

        public static IServiceCollection AddHostedHandler(
            this IServiceCollection services, 
            Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            return services.AddSingleton<IHostedService>(sp => new DelegateHandlerHostService(sp, handlerDelegate));
        }

    }
}