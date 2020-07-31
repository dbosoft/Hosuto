using System;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public interface IInternalModuleHost : IModuleHost
    {
        void Bootstrap(IServiceProvider moduleHostServices);

    }

    public interface IInternalModuleHost<TModule> : IModuleHost<TModule>,  IInternalModuleHost where TModule : IModule
    {

    }
    
}