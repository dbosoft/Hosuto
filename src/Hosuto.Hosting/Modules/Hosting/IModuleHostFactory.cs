using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHostFactory
    {
        IModuleHost CreateModuleHost(Dictionary<Type, Action<IHostBuilder>> modules, 
            ModuleHostBuilderSettings builderSettings, 
            IServiceProvider serviceProvider);
    }

}