using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.HostedServices
{
    public interface IHostedServiceHandler
    {
        Task Execute(CancellationToken stoppingToken);
    }
}