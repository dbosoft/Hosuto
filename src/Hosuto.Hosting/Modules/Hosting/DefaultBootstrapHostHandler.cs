using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultBootstrapHostHandler<TModule> where TModule : IModule
    {

        public IModuleBootstrapContext<TModule> BootstrapContext { get; set; }

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

        protected virtual void ApplyConfiguration(IHostBuilder builder)
        {
            
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();
            var moduleAssemblyName = BootstrapContext.Module.GetType().Assembly.GetName().Name;

            builder.ConfigureHostConfiguration((configure) =>
            {
                configure.AddConfiguration(hostBuilderContext.Configuration);
                configure.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(HostDefaults.ApplicationKey,moduleAssemblyName)
                });

            });

            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                Filters.BuildFilterPipeline(
                    BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationFilter>(),
                    (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(ctx), config);

            });

        }


        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder();

            return builder;
        }

        protected virtual IModuleContext<TModule> CreateModuleContext(IServiceProvider services)
        {
            var factory = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<IModuleContextFactory<TModule>>();
            return factory.CreateModuleContext(BootstrapContext, services);
        }
        
        public virtual void BootstrapHost(BootstrapModuleHostCommand<TModule> command)
        {
            if (command.Host != null)
                return;

            BootstrapContext = command.BootstrapContext;

            var builder = CreateHostBuilder();
            ApplyConfiguration(builder);
            builder.ConfigureServices((ctx, services) =>
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var tempServiceProvider = services.BuildServiceProvider())
                {
                    ConfigureServices(ctx, services, tempServiceProvider);
                }
            });


            command.Options.ConfigureBuilderAction?.Invoke(builder);
            command.Host = builder.Build();
            command.ModuleContext = CreateModuleContext(command.Host.Services);

        }
    }
}