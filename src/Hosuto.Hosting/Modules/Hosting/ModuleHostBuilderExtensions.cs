using System;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
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
            return builder.Build().RunAsync();

        }


        /// <summary>
        /// Sets up the configuration for the remainder of the build process and application. This can be called multiple times and
        /// the results will be additive. The results will be available at <see cref="HostBuilderContext.Configuration"/> for
        /// subsequent operations, as well as in <see cref="IHost.Services"/>.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
        /// to construct the <see cref="IConfiguration"/> for the application.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        static IModuleHostBuilder ConfigureModuleConfiguration(
            this IModuleHostBuilder hostBuilder,
            Action<IModuleHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleConfigurationConfigurer>(sp =>
                    new DelegateModuleHostBuilderConfigurationConfigurer(configureDelegate));
            });

            return hostBuilder;
        }

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IServiceCollection"/> that will be used
        /// to construct the <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IModuleHostBuilder ConfigureServices(
            this IModuleHostBuilder hostBuilder,
            Action<IModuleHostBuilderContext, IServiceCollection> configureDelegate)
        {
            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleServicesConfigurer>(sp =>
                    new DelegateModuleServicesConfigurer(configureDelegate));
            });

            return hostBuilder;
        }

    }
}