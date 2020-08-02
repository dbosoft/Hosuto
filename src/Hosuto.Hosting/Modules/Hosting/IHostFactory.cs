using System;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IHostFactory
    {
        (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, ModuleHostingOptions options) where TModule : IModule;
    }

}