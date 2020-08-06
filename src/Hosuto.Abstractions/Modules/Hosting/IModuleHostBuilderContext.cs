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
}