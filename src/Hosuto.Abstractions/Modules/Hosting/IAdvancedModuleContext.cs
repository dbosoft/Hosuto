using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IAdvancedModuleContext
    {
        IServiceProvider FrameworkServices { get; }
        IServiceProvider HostServices { get; }

        object RootContext { get; }

    }
}