using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModulesHostBuilderExtensions
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IModulesHostBuilder UseSimpleInjector(this IModulesHostBuilder hostBuilder,
            Container container, bool enableModuleContainer = true) 
        {
            if (container.Options.DefaultScopedLifestyle == null)
            {
                container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            }

            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddSingleton(container);
                services.AddTransient<IModuleHostServiceProviderFactory, ServiceProviderFactory>();
            });

            return !enableModuleContainer 
                ? hostBuilder 
                : UseSimpleInjector(hostBuilder);

        }

        public static IModulesHostBuilder UseSimpleInjector(this IModulesHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IModuleContextFactory<>), typeof(ModuleContextFactory<>));
                services.AddTransient(typeof(IModuleServicesConfigurer), typeof(ServicesStartupConfigurer));
                services.Decorate<IHostFactory, HostFactoryDecorator>();

            });

            return hostBuilder;
        }

    }
}