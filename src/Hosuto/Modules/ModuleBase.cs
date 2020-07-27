using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules
{

    public abstract class ModuleBase : IModule
    {
        public abstract string Name { get; }


        public abstract void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services);


    }
}
