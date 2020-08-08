using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleConfigurationFilter : IFilter<IModulesHostBuilderContext, IConfigurationBuilder>
    {
    }


}