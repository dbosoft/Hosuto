using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ModuleHost<TModule> : IModuleHost<TModule> where TModule : IModule
    {
        private readonly IServiceProvider _frameworkServices;
        private IHost _host;
        private bool _bootstrapRun;
        // ReSharper disable once MemberCanBePrivate.Global
        
        public ModuleHost(IServiceProvider frameworkServices)
        {
            _frameworkServices = frameworkServices;
        }

        public virtual void Bootstrap(IServiceProvider moduleHostServices, ModuleHostBootstrapActions bootstrapActions)
        {
            if (_bootstrapRun) return;
            _bootstrapRun = true;

            var module = moduleHostServices.GetRequiredService<TModule>();
            var contextFactory = _frameworkServices.GetRequiredService<IModuleContextFactory<TModule>>();
            var context = contextFactory.CreateModuleBootstrapContext(module, moduleHostServices, _frameworkServices);
            
            var hostFactory = _frameworkServices.GetRequiredService<IHostFactory>();
            (_host, ModuleContext) = hostFactory.CreateHost(context, bootstrapActions.ConfigureBuilder);

            if (ModuleContext.Advanced.HostServices.GetService<IModuleContextAccessor>() is ModuleContextAccessor contextAccessor) 
                contextAccessor.Context = ModuleContext;

            bootstrapActions.Bootstrap?.Invoke(_host.Services);
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return _host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _host.StopAsync(cancellationToken).ConfigureAwait(false);
            ModuleContext.Dispose();
        }

        public IServiceProvider Services => _host.Services;

        public Task WaitForShutdownAsync(CancellationToken cancellationToken) => _host.WaitForShutdownAsync(cancellationToken);

        public void Dispose()
        {
            _host?.Dispose();
            ModuleContext?.Dispose();

            ModuleContext = null;
            _host = null;
        }

        public IModuleContext<TModule> ModuleContext { get; private set;  }
    }
}