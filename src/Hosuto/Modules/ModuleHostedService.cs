using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    internal class ModuleHostedService<THostedService> : IHostedService where THostedService : class, IHostedService
    {
        private readonly THostedService _innerService;

        public ModuleHostedService(IServiceProvider serviceProvider)
        {
            _innerService = serviceProvider.AsModuleServices().GetRequiredService<THostedService>();
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