using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.HostedServices
{
    public interface IHostedServiceHandler
    {
        Task Execute(IServiceProvider serviceProvider, CancellationToken stoppingToken);
    }
}