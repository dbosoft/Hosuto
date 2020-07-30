using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IModuleHostBuilder UseSimpleInjector(this IModuleHostBuilder hostBuilder,
            Container container, bool enableModuleContainer = true) 
        {
            hostBuilder.UseServiceProviderFactory(new ServiceProviderFactory(container));

            return !enableModuleContainer 
                ? hostBuilder 
                : UseSimpleInjector(hostBuilder);

        }

        public static IModuleHostBuilder UseSimpleInjector(this IModuleHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IModuleStartupContextFactory<>), typeof(ModuleStartupContextFactory<>));
                services.AddTransient(typeof(IServicesStartupConfigurer<>), typeof(ServicesStartupConfigurer<>));
                services.Decorate<IHostFactory, HostFactoryDecorator>();

            });

            return hostBuilder;
        }

    }
}