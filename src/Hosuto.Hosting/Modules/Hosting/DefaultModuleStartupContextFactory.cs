using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultModuleStartupContextFactory<TModule> : IModuleStartupContextFactory<TModule> where TModule : IModule
    {

        public ModuleStartupContext<TModule> CreateStartupContext(TModule module, ModuleHostBuilderSettings builderSettings, IServiceProvider serviceProvider)
        {
            return new ModuleStartupContext<TModule>(module, builderSettings, serviceProvider);
        }

    }
}