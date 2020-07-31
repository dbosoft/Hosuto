using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHost : IHost
    {
        IServiceProvider ModuleHostServices { get;  }
        Task WaitForShutdownAsync(CancellationToken cancellationToken = default);
    }

    public interface IModuleHost<TModule> : IModuleHost where TModule : IModule
    {
        IModuleContext<TModule> ModuleContext { get;  }
    } 
}