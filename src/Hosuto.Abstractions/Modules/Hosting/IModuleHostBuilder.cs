using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModulesHostBuilder: IHostBuilder
    {

        IModulesHostBuilder HostModule<TModule>(Action<IModuleHostingOptions> options = null) where TModule : class;
        IModulesHostBuilder HostModule(Type moduleType, Action<IModuleHostingOptions> options = null);

        /// <summary>
        /// Direct access to the host builder used to build the internal host. This could be used
        /// in special cases when you would like to apply configuration only to the internal host and not to the modules.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IHostBuilder"/> that will be used
        /// to construct the internal host.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        IModulesHostBuilder ConfigureInternalHost(Action<IHostBuilder> configureDelegate);

        /// <summary>
        /// Set up the configuration for the builder itself. This will be used to initialize the <see cref="IHostEnvironment"/>
        /// for use later in the build process. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
        /// to construct the <see cref="IConfiguration"/> for the host.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate);

        /// <summary>
        /// Sets up the configuration for the remainder of the build process and application. This can be called multiple times and
        /// the results will be additive. The results will be available at <see cref="HostBuilderContext.Configuration"/> for
        /// subsequent operations, as well as in <see cref="IHost.Services"/>.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
        /// to construct the <see cref="IConfiguration"/> for the application.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IServiceCollection"/> that will be used
        /// to construct the <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);


        /// <summary>
        /// Adds services to the framework container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IServiceCollection"/> that will be used
        /// to construct the internal <see cref="IServiceProvider"/> used to configure the module builder.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        IModulesHostBuilder ConfigureFrameworkServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);



        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of builder.</typeparam>
        /// <param name="factory">The factory to register.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory);

#if NETSTANDARD2_1
        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of builder.</typeparam>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory);
#endif
        /// <summary>
        /// Enables configuring the instantiated dependency container. This can be called multiple times and
        /// the results will be additive.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of builder.</typeparam>
        /// <param name="configureDelegate">The delegate which configures the builder.</param>
        /// <returns>The same instance of the <see cref="IModulesHostBuilder"/> for chaining.</returns>
        new IModulesHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate);


    }

}