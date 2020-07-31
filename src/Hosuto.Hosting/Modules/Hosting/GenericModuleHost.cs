﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once UnusedMember.Global
    public class ModuleHost<TModule> : IModuleHost<TModule> where TModule : IModule
    {
        private readonly Action<IHostBuilder> _configureHostBuilderAction;
        private IHost _host;
        private readonly ModuleStartupContext<TModule> _startupContext;
        private bool _bootstrapRun;
        
        public ModuleHost(TModule module,
            ModuleHostBuilderSettings builderSettings,
            Action<IHostBuilder> configureHostBuilderAction,
            IServiceProvider serviceProvider)
        {
            _configureHostBuilderAction = configureHostBuilderAction;
            var startupContextFactory = builderSettings.FrameworkServiceProvider.GetService<IModuleStartupContextFactory<TModule>>() ?? new DefaultModuleStartupContextFactory<TModule>();
            _startupContext = startupContextFactory.CreateStartupContext(module, builderSettings, serviceProvider) ??
                              new ModuleStartupContext<TModule>(module, builderSettings, serviceProvider);
        }

        public void Bootstrap()
        {
            if (_bootstrapRun) return;
            _bootstrapRun = true;

            var hostFactory = _startupContext.BuilderSettings.FrameworkServiceProvider.GetRequiredService<IHostFactory>();
            _host = hostFactory.CreateHost(_startupContext, _configureHostBuilderAction);
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            Bootstrap();
            return _host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _host.StopAsync(cancellationToken).ConfigureAwait(false);
            _startupContext.Dispose();
        }

        public IServiceProvider Services => _host?.Services;

        public Task WaitForShutdownAsync(CancellationToken cancellationToken = default) => _host.WaitForShutdownAsync(cancellationToken);

        public void Dispose()
        {
            _startupContext?.Dispose();
            _host?.Dispose();
        }
    }
}