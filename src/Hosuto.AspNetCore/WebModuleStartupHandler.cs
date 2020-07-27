using System;
using System.Collections.Generic;
using System.Linq;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Hosuto.AspNetCore.Hosting
{
    public class WebModuleStartupHandler<TModule> : DefaultModuleStartupHandler<TModule> where TModule : IModule
    {

        public WebModuleStartupHandler(ModuleStartupContext<TModule> startupContext) : base(startupContext)
        {
            if (!(startupContext.Module is IWebModule))
                throw new InvalidOperationException(
                    $"WebModuleStartupHandler requires a WebModule in {nameof(startupContext)}");
        }


        protected virtual void Configure(IApplicationBuilder applicationBuilder)
        {
            CallOptionalMethod(StartupContext.Module, "Configure",
                StartupContext.ServiceProvider,
                applicationBuilder.ApplicationServices,
                applicationBuilder);

        }

        public override IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction)
        {

            var hostBuilderConfigurers = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetServices<IWebModuleWebHostBuilderConfigurer>();


#if NETCOREAPP
            
            var webHostBuilderInitializer = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetService<IWebModuleWebHostBuilderInitializer>();

            if (webHostBuilderInitializer == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var builder = new HostBuilder();

            foreach (var configureAction in StartupContext.BuilderSettings.ConfigurationActions)
            {
                builder.ConfigureAppConfiguration(configureAction);
            }

            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
            {
                builder.ConfigureServices(configureServicesAction);
            }

            builder.ConfigureServices(ConfigureServices);
            
            webHostBuilderInitializer.ConfigureWebHost(StartupContext.Module as IWebModule, builder, 
                hostBuilderConfigurers.Append(new DelegateWebHostBuilderConfigurer((_, webHostBuilder)=> webHostBuilder.Configure(Configure))));
            
            configureHostBuilderAction?.Invoke(builder);
            
            return builder.Build();
#else

            var webHostBuilderFactory = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetService<IWebModuleWebHostBuilderFactory>();

            if (webHostBuilderFactory == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var webHostBuilder = webHostBuilderFactory.CreateWebHost(StartupContext.Module as IWebModule);

            foreach (var hostBuilderConfigurer in hostBuilderConfigurers)
            {
                hostBuilderConfigurer.ConfigureWebHost(StartupContext.Module as IWebModule, webHostBuilder);
            }

            foreach (var configureAction in StartupContext.BuilderSettings.ConfigurationActions)
            {
                webHostBuilder.ConfigureAppConfiguration((webCtx, configure) =>
                    configureAction(WebContextToHostBuilderContext(webCtx), configure));
            }

            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
            {
                webHostBuilder.ConfigureServices(srv => configureServicesAction(StartupContext.BuilderSettings.HostBuilderContext, srv));
            }
            
            var webHost = webHostBuilder.ConfigureServices(ConfigureServices)
                    .Configure(Configure)
                    .Build();

            return new WebHostWrapperHost(webHost);

#endif

        }

        private static HostBuilderContext WebContextToHostBuilderContext(WebHostBuilderContext webContext)
        {
            return new HostBuilderContext(new Dictionary<object, object>{ {"WebHostBuilderContext", webContext} })
            {
                Configuration =  webContext.Configuration, 
                HostingEnvironment = new HostingEnvironment
                {
                    ApplicationName = webContext.HostingEnvironment.ApplicationName,
                    ContentRootFileProvider = webContext.HostingEnvironment.ContentRootFileProvider,
                    ContentRootPath = webContext.HostingEnvironment.ContentRootPath, 
                    EnvironmentName = webContext.HostingEnvironment.EnvironmentName,
                }
            };
        }


    }
}