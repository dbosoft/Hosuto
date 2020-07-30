using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
        public static IModuleHostBuilder UseSimpleInjector(this IModuleHostBuilder hostBuilder,
            Container container)
        {
            hostBuilder.UseServiceProviderFactory(new ServiceProviderFactory(container));
            return hostBuilder;
        }


    }
}