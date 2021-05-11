using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModulesHostService<TModule> : IHostedService where TModule : class
    {
        private readonly IModuleHost<TModule> _moduleHost;

        public ModulesHostService(IModuleHost<TModule> moduleHost)
        {
            _moduleHost = moduleHost;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _moduleHost.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _moduleHost.StopAsync(cancellationToken);
        }

    }
}