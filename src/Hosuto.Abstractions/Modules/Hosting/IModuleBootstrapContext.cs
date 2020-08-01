using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleBootstrapContext<TModule> where TModule : IModule
    {
        TModule Module { get; }
        IServiceProvider ModuleHostServices { get; }

        IAdvancedModuleContext Advanced { get;  }
    }
}