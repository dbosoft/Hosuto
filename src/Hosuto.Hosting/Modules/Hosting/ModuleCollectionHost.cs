using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModuleCollectionHost : IModuleHost
    {
        private readonly IEnumerable<IModuleHost> _moduleHosts;

        public ModuleCollectionHost(IEnumerable<IModuleHost> moduleHosts)
        {
            _moduleHosts = moduleHosts.ToArray();
        }

        public void Bootstrap()
        {
            foreach (var moduleHost in _moduleHosts)
            {
                moduleHost.Bootstrap();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            foreach (var moduleHost in _moduleHosts)
            {
                await moduleHost.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            foreach (var moduleHost in _moduleHosts)
            {
                await moduleHost.StopAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public IServiceProvider Services => _moduleHosts.FirstOrDefault()?.Services;
            
        public async Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
        {
            foreach (var moduleHost in _moduleHosts)
            {
                await moduleHost.WaitForShutdownAsync(cancellationToken);
            }

        }

        public void Dispose()
        {
            foreach (var moduleHost in _moduleHosts)
            {
                moduleHost.Dispose();
            }
        }
    }
}