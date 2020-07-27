using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public interface IModuleStartupHandler
    { 
        IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction);
    }
}