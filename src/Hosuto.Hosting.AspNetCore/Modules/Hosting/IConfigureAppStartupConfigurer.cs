using System;
using Microsoft.AspNetCore.Builder;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IConfigureAppConfigurer<TModule> where TModule : IModule
    {
        void Configure(IModuleContext<TModule> context, IApplicationBuilder app);
    }
}