using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IModuleContext<TModule> : IDisposable where TModule : IModule
    {
        TModule Module { get; }
        IServiceProvider ModuleHostServices { get; }

        IServiceProvider Services { get; }

        IAdvancedModuleContext Advanced { get; }

    }

    public interface IAdvancedModuleContext
    {
        IServiceProvider FrameworkServices { get; }
        object RootContext { get; }

    }
}