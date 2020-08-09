using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleConfigurationFilter : IFilter<IModulesHostBuilderContext, IConfigurationBuilder>
    {
    }

    public interface IModuleConfigurationFilter<TModule> : IFilter<IModulesHostBuilderContext<TModule>, IConfigurationBuilder> where TModule : IModule
    {
    }

}