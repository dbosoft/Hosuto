using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;

namespace Dbosoft.Hosuto.HostedServices
{
    internal class DelegateServiceHandler : IHostedServiceHandler
    {
        private readonly Func<IServiceProvider, CancellationToken, Task> _handlerDelegate;

        public DelegateServiceHandler(Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            _handlerDelegate = handlerDelegate;
        }


        public Task Execute(IServiceProvider serviceProvider, CancellationToken stoppingToken) => _handlerDelegate(serviceProvider, stoppingToken);
    }
}