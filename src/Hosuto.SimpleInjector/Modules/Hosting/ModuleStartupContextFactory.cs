using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class ModuleStartupContextFactory<TModule> : IModuleStartupContextFactory<TModule> where TModule: IModule
    {
        public ModuleStartupContext<TModule> CreateStartupContext(TModule module, ModuleHostBuilderSettings builderSettings,
            IServiceProvider serviceProvider)
        {
            return new ContainerModuleStartupContext<TModule>(module, builderSettings, serviceProvider);
        }
    }
}