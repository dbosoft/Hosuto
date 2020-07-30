using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleStartupContextFactory<TModule> where TModule : IModule
    {
        ModuleStartupContext<TModule> CreateStartupContext(TModule module, ModuleHostBuilderSettings builderSettings, IServiceProvider serviceProvider);

    }
}