using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleBootstrapContext<TModule> where TModule : IModule
    {
        TModule Module { get; }
        IServiceProvider ModuleHostServices { get; }

        IAdvancedModuleContext Advanced { get;  }
    }


    public interface IModuleHostBuilderContext
    {
        IModule Module { get; }
        IServiceProvider ModuleHostServices { get; }

        HostBuilderContext HostBuilderContext { get; }

        IAdvancedModuleContext Advanced { get; }

    }
}