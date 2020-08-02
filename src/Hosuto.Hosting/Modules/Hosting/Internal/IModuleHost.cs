using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public interface IModuleHost : IHost
    {
        void Bootstrap(IServiceProvider moduleHostServices, ModuleHostingOptions options);

    }

    public interface IModuleHost<TModule> : Hosting.IModuleHost<TModule>,  IModuleHost where TModule : IModule
    {

    }
    
}