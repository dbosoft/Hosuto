using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleBootstrapContext<TModule> where TModule : class
    {
        TModule Module { get; }
        IServiceProvider ModulesHostServices { get; }

        IAdvancedModuleContext Advanced { get;  }
    }
}