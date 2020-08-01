using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public interface IModuleHost : IHost
    {
        void Bootstrap(IServiceProvider moduleHostServices, ModuleHostBootstrapActions bootstrapActions);

    }

    public interface IModuleHost<TModule> : Hosting.IModuleHost<TModule>,  IModuleHost where TModule : IModule
    {

    }
    
}