using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public interface IModuleStartupContextFactory<TModule> where TModule : IModule
    {
        ModuleStartupContext<TModule> CreateStartupContext(TModule module, ModuleHostBuilderSettings builderSettings, IServiceProvider serviceProvider);

    }
}