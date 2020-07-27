using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public interface IModuleHost : IHost
    {
        void Bootstrap();
        Task WaitForShutdownAsync(CancellationToken cancellationToken = default);
    }

    public interface IModuleHost<TModule> : IModuleHost where TModule : IModule
    {
    }
}