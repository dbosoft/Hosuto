using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.Modules
{
    internal class DelegateServiceHandler : IHostedServiceHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, CancellationToken, Task> _handlerDelegate;

        public DelegateServiceHandler(IServiceProvider serviceProvider, Func<IServiceProvider, CancellationToken, Task> handlerDelegate)
        {
            _serviceProvider = serviceProvider;
            _handlerDelegate = handlerDelegate;
        }


        public Task Execute(CancellationToken stoppingToken) => _handlerDelegate(_serviceProvider, stoppingToken);
    }
}