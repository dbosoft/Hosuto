using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IModuleContext<TModule> : IModuleContext where TModule : IModule
    {
        new TModule Module { get; }

    }


    public interface IModuleContext : IDisposable
    {
        object Module { get; }
        IServiceProvider ModuleHostServices { get; }

        IServiceProvider Services { get; }

        IAdvancedModuleContext Advanced { get; }

    }
}