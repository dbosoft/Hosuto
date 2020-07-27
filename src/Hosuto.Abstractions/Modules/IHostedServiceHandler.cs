using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.Modules
{
    public interface IHostedServiceHandler
    {
        Task Execute(CancellationToken stoppingToken);
    }
}