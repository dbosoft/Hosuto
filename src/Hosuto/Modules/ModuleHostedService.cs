using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    internal class ModuleHostedService<THostedService> : IHostedService where THostedService : class, IHostedService
    {
        private readonly THostedService _innerService;

        public ModuleHostedService(THostedService hostedService)
        {
            _innerService = hostedService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _innerService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _innerService.StopAsync(cancellationToken);
        }
    }
}