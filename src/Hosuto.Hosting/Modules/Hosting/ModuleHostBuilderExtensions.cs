using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
        public static IModuleHostBuilder UseServiceCollection(this IModuleHostBuilder hostBuilder,
            IServiceCollection services)
        {
            hostBuilder.UseServiceProviderFactory(new ServiceCollectionServiceProviderFactory(services));
            return hostBuilder;
        }


        public static Task RunModule<TModule>(this IModuleHostBuilder hostBuilder,
            Action<IHostBuilder> hostConfigureAction = null) where TModule : class, IModule
        {
            return RunModule<TModule>(hostBuilder, null, hostConfigureAction);
        }


        public static Task RunModule<TModule>(this IModuleHostBuilder hostBuilder, 
            Action<IModuleHostBuilder> configureAction) where TModule : class, IModule
        {
            return RunModule<TModule>(hostBuilder, configureAction, null);

        }

        public static Task RunModule<TModule>(this IModuleHostBuilder builder,
            Action<IModuleHostBuilder> configureAction,
            Action<IHostBuilder> hostConfigureAction) where TModule : class, IModule
        {
            builder.HostModule<TModule>(hostConfigureAction);
            configureAction?.Invoke(builder);
            return builder.Build().Run();

        }
    }
}