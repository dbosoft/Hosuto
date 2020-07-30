using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultHostFactory : IHostFactory
    {
        public IHost CreateHost<TModule>(ModuleStartupContext<TModule> startupContext, Action<IHostBuilder> configureHostBuilderAction) where TModule : IModule
        {
            var factory = new DefaultHostFactory<TModule>(startupContext);
            return factory.CreateHost(configureHostBuilderAction);
        }
    }

    public class DefaultHostFactory<TModule> where TModule : IModule
    {
        public DefaultHostFactory(ModuleStartupContext<TModule> startupContext)
        {
            StartupContext = startupContext;
        }

        public ModuleStartupContext<TModule> StartupContext { get; }
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            foreach (var configurer in StartupContext.BuilderSettings.FrameworkServiceProvider.GetServices<IServicesStartupConfigurer<TModule>>())
            {
                configurer.ConfigureServices(StartupContext, services);
            }

            var tempProvider = services.BuildServiceProvider();


            ModuleMethodInvoker.CallOptionalMethod(StartupContext.Module, "ConfigureServices", StartupContext.ServiceProvider, tempProvider, services);

        }


        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder();

            builder.ConfigureHostConfiguration((configure) =>
            {
                configure.Sources.Clear();
                configure.AddConfiguration(StartupContext.BuilderSettings.HostBuilderContext.Configuration);
            });

            builder.UseContentRoot(GetContentRoot());

            var moduleAssemblyName = StartupContext.Module.GetType().Assembly.GetName().Name;
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;
            });

            foreach (var configureAction in StartupContext.BuilderSettings.ConfigurationActions)
            {
                builder.ConfigureAppConfiguration(configureAction);
            }

            return builder;
        }

        protected virtual string GetContentRoot()
        {
            var pathCandidate = Path.Combine(
                StartupContext.BuilderSettings.HostBuilderContext.HostingEnvironment
                    .ContentRootPath, "..", StartupContext.Module.Name);

            if (!Directory.Exists(pathCandidate))
                return StartupContext.BuilderSettings.HostBuilderContext.HostingEnvironment
                    .ContentRootPath;


            return Path.GetFullPath(pathCandidate);

        }

        public virtual IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction)
        {
            var builder = CreateHostBuilder();

            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
            {
                builder.ConfigureServices(configureServicesAction);
            }

            builder.ConfigureServices(ConfigureServices);
            configureHostBuilderAction?.Invoke(builder);
            var host = builder.Build();

            return host;
        }
    }
}