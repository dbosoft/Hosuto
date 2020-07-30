using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IServicesConfigurer<TModule> where TModule : IModule
    {
        void ConfigureServices(TModule module, IServiceProvider serviceProvider, IServiceCollection services);
    }
}