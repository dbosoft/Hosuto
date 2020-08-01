using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.HostedServices
{
    internal class DelegateHandlerHostService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, CancellationToken, Task> _handlerDelegate;

        public DelegateHandlerHostService(IServiceProvider serviceProvider, Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            _serviceProvider = serviceProvider;
            _handlerDelegate = handlerDelegate;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var services = _serviceProvider.AsModuleServices();
            using (var scope = services.CreateScope())
            {
                return _handlerDelegate(scope.ServiceProvider, stoppingToken);
            }
        }
    }
}