using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleConfigurationConfigurer
    {
        void ConfigureModuleConfiguration(IModuleHostBuilderContext context, IConfigurationBuilder configuration);
    }


}