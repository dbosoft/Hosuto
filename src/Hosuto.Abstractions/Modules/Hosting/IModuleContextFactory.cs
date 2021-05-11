using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleContextFactory<TModule> where TModule : class
    {
        IModuleContext<TModule> CreateModuleContext(IModuleBootstrapContext<TModule> bootstrapContext, IServiceProvider moduleServices);
        IModuleBootstrapContext<TModule> CreateModuleBootstrapContext(TModule module, IServiceProvider moduleHostServices, IServiceProvider frameworkServices);

    }
}