using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
        public static IModuleHostBuilder UseAspNetCoreWithSimpleInjector(this IModuleHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IConfigureAppStartupConfigurer<>), typeof(ConfigureAppStartupConfigurer<>));

            });

            return hostBuilder;
        }

    }
}