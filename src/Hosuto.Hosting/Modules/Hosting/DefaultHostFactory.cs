using System;
using System.IO;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultHostFactory : IHostFactory
    {
        public (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, Action<IHostBuilder> configure) where TModule : IModule
        {
            var factory = new DefaultHostFactory<TModule>(bootstrapContext);
            return factory.CreateHost(configure);
        }
        
    }

    public class DefaultHostFactory<TModule> where TModule : IModule
    {
        public DefaultHostFactory(IModuleBootstrapContext<TModule> bootstrapContext)
        {
            BootstrapContext = bootstrapContext;
        }

        public IModuleBootstrapContext<TModule> BootstrapContext { get; }

        protected virtual void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            foreach (var configurer in BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleServicesConfigurer>())
            {
                configurer.ConfigureServices(BootstrapContext.ToModuleHostBuilderContext(hostBuilderContext), services);
            }

            var tempProvider = services.BuildServiceProvider();


            ModuleMethodInvoker.CallOptionalMethod(BootstrapContext, "ConfigureServices", tempProvider, services);

        }


        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder();
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();

            builder.ConfigureHostConfiguration((configure) =>
            {
                configure.Sources.Clear();
                configure.AddConfiguration(hostBuilderContext.Configuration);
            });

            builder.UseContentRoot(GetContentRoot());

            var moduleAssemblyName = BootstrapContext.Module.GetType().Assembly.GetName().Name;

            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;

                foreach (var configurer in BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationConfigurer>())
                {
                    configurer.ConfigureModuleConfiguration(BootstrapContext.ToModuleHostBuilderContext(ctx), config);
                }
            });

            return builder;
        }

        protected virtual string GetContentRoot()
        {
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();

            var pathCandidate = Path.Combine(hostBuilderContext.HostingEnvironment
                    .ContentRootPath, "..", BootstrapContext.Module.Name);

            if (!Directory.Exists(pathCandidate))
                return hostBuilderContext.HostingEnvironment.ContentRootPath;


            return Path.GetFullPath(pathCandidate);

        }

        protected virtual IModuleContext<TModule> CreateModuleContext(IServiceProvider services)
        {
            var factory = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<IModuleContextFactory<TModule>>();
            return factory.CreateModuleContext(BootstrapContext, services);
        }

        public virtual (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost(Action<IHostBuilder> configure)
        {
            var builder = CreateHostBuilder();

            builder.ConfigureServices(ConfigureServices);
            configure?.Invoke(builder);
            var host = builder.Build();
            
            
            return (host, CreateModuleContext(host.Services));
        }
    }
}