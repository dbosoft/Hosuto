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
    public class DefaultBootstrapHostHandler<TModule> where TModule : class
    {

        public IModuleBootstrapContext<TModule> BootstrapContext { get; set; }

        protected virtual void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services, IServiceProvider serviceProvider)
        {
            IFilter<IModulesHostBuilderContext, IServiceCollection> moduleServicesFilterAdapter =
                new GenericModuleHostBuilderContextAdapter<IModuleServicesFilter<TModule>, TModule, IServiceCollection>();

            Filters.BuildFilterPipeline(
                BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleServicesFilter>()
                    .Append(moduleServicesFilterAdapter)
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

        protected static void ConfigureServiceProviderValidation(IHostBuilder builder, ModuleHostingOptions options)
        {
            if (!options.HasServiceProviderValidationOverride())
                return;

#if !NETSTANDARD2_0
            builder.UseDefaultServiceProvider((_, serviceProviderOptions) =>
                ApplyServiceProviderValidation(serviceProviderOptions, options));
#endif
        }

        protected static void ApplyServiceProviderValidation(ServiceProviderOptions serviceProviderOptions, ModuleHostingOptions options)
        {
            var validateScopes = options.GetValidateScopesOverride();
            if (validateScopes != null)
                serviceProviderOptions.ValidateScopes = validateScopes.Value;
#if !NETSTANDARD2_0
            var validateOnBuild = options.GetValidateOnBuildOverride();
            if (validateOnBuild != null)
                serviceProviderOptions.ValidateOnBuild = validateOnBuild.Value;
#endif
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

            ConfigureServiceProviderValidation(builder, command.Options);

            command.Options.ConfigureBuilderAction?.Invoke(builder);
            command.Host = builder.Build();
            command.ModuleContext = CreateModuleContext(command.Host.Services);

        }
    }
}