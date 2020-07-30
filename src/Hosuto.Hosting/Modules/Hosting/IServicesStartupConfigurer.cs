using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IServicesStartupConfigurer<TModule> where TModule : IModule
    {
        void ConfigureServices(ModuleStartupContext<TModule> startupContext, IServiceCollection services);
    }
}