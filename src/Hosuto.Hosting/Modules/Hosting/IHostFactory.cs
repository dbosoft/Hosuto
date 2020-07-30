using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IHostFactory
    {
        IHost CreateHost<TModule>(ModuleStartupContext<TModule> startupContext,
            Action<IHostBuilder> configureHostBuilderAction) where TModule : IModule;
    }

}