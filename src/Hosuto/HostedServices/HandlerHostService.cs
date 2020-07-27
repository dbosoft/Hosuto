using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.HostedServices
{
    internal class HandlerHostService<TServiceHandler> : BackgroundService where TServiceHandler : class, IHostedServiceHandler
    {
        private readonly TServiceHandler _serviceHandler;

        public HandlerHostService(TServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _serviceHandler.Execute(stoppingToken);
        }
    }
}