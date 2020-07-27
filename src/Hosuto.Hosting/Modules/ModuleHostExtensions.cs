using System.Threading;
using System.Threading.Tasks;

namespace Dbosoft.Hosuto.Modules
{
    public static class ModuleHostExtensions
    {
        public static async Task Run(this IModuleHost moduleHost, CancellationToken cancellationToken = default)
        {
            moduleHost.Bootstrap();
            await moduleHost.StartAsync(cancellationToken).ConfigureAwait(false);
            await moduleHost.WaitForShutdownAsync(cancellationToken).ConfigureAwait(false);
            await moduleHost.StopAsync(cancellationToken).ConfigureAwait(false);

        }
    }
}