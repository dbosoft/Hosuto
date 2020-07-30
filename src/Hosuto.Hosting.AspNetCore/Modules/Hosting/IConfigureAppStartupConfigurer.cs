using System;
using Microsoft.AspNetCore.Builder;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IConfigureAppStartupConfigurer<TModule> where TModule : IModule
    {
        void Configure(ModuleStartupContext<TModule> startupContext, IApplicationBuilder app);
    }
}