using System.Diagnostics;
using System.Threading.Tasks;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules
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