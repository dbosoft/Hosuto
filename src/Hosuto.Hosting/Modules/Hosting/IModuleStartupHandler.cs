using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleStartupHandler
    { 
        IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction);
    }
}