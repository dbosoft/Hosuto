using System;
using System.IO;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultHostFactory : IHostFactory
    {
        public (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, ModuleHostingOptions options) where TModule : IModule
        {
            var factory = new DefaultHostFactory<TModule>(bootstrapContext);
            return factory.CreateHost(options);
        }
        
    }

    public class DefaultHostFactory<TModule> where TModule : IModule
    {
        public DefaultHostFactory(IModuleBootstrapContext<TModule> bootstrapContext)
        {
            BootstrapContext = bootstrapContext;
        }

        public IModuleBootstrapContext<TModule> BootstrapContext { get; }

        protected virtual void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services, IServiceProvider serviceProvider)
        {
            Filters.BuildFilterPipeline(
                BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleServicesFilter>()
                    .Append(GenericModuleHostBuilderContextAdapter<IServiceCollection>.Create(typeof(IModuleServicesFilter<>)))
                , (ctx, s) =>
                {
                    ModuleMethodInvoker.CallOptionalMethod(BootstrapContext.ToModuleContext(serviceProvider), "ConfigureServices", serviceProvider, s);
                })
                (BootstrapContext.ToModuleHostBuilderContext(hostBuilderContext), services);
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

                Filters.BuildFilterPipeline(
                    BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationFilter>(),
                    (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(ctx), config);

            });

            
            builder.ConfigureServices((ctx, services) =>
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using(var tempServiceProvider = services.BuildServiceProvider())
                {
                    ConfigureServices(ctx, services, tempServiceProvider);
                }
            });

            return builder;
        }

        protected virtual string GetContentRoot()
        {
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();

            if (BootstrapContext.Module.Name == null)
                return hostBuilderContext.HostingEnvironment.ContentRootPath;

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

        public virtual (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost(ModuleHostingOptions options)
        {
            var builder = CreateHostBuilder();

            options.ConfigureBuilderAction?.Invoke(builder);
            var host = builder.Build();

            var context = CreateModuleContext(host.Services);
            return (host, context);
        }
    }
}