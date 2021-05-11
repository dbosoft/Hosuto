using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModulesHostBuilderContext
    {
        object Module { get; }
        IServiceProvider ModulesHostServices { get; }

        HostBuilderContext HostBuilderContext { get; }

        IAdvancedModuleContext Advanced { get; }

    }

    public interface IModulesHostBuilderContext<TModule> : IModulesHostBuilderContext where TModule : class
    {
        new TModule Module { get; }
    }
}