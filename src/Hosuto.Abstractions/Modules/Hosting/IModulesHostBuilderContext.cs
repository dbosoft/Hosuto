using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModulesHostBuilderContext
    {
        IModule Module { get; }
        IServiceProvider ModulesHostServices { get; }

        HostBuilderContext HostBuilderContext { get; }

        IAdvancedModuleContext Advanced { get; }

    }

    public interface IModulesHostBuilderContext<TModule> : IModulesHostBuilderContext where TModule : IModule
    {
        new TModule Module { get; }
    }
}