using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleServicesConfigurer
    {
        void ConfigureServices(IModulesHostBuilderContext context, IServiceCollection services);
    }
}