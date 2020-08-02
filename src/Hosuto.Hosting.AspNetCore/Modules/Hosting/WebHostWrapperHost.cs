#if NETSTANDARD

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{

    internal class WebHostWrapperHost : IHost
    {
        public WebHostWrapperHost(IWebHost webHost)
        {
            WebHost = webHost;
        }

        public IWebHost WebHost { get; set; }

        public void Dispose() => WebHost.Dispose();

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) => WebHost.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) => WebHost.StopAsync(cancellationToken);

        public IServiceProvider Services => WebHost.Services;
    }

}

#endif