using System;
#if NETCOREAPP
using System.Linq;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if NETSTANDARD
using System.Collections.Generic;
using Microsoft.Extensions.Hosting.Internal;
#endif

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WebModuleHostFactory : IHostFactory
    {
        private readonly IHostFactory _decoratedHostFactory;

        public WebModuleHostFactory(IHostFactory decoratedHostFactory)
        {
            _decoratedHostFactory = decoratedHostFactory;
        }

        public IHost CreateHost<TModule>(ModuleStartupContext<TModule> startupContext, Action<IHostBuilder> configureHostBuilderAction) where TModule : IModule
        {
            switch (startupContext.Module)
            {
               case WebModule _:
                    {
                        var factory = new WebModuleHostFactory<TModule>(startupContext);
                        return factory.CreateHost(configureHostBuilderAction);
                    }
                    
                default:
                    return _decoratedHostFactory.CreateHost(startupContext, configureHostBuilderAction);
            }
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class WebModuleHostFactory<TModule> : DefaultHostFactory<TModule> where TModule : IModule
    {
        public WebModuleHostFactory(ModuleStartupContext<TModule> startupContext) : base(startupContext)
        {
        }

        protected virtual void Configure(IApplicationBuilder app)
        {
            foreach (var configurer in StartupContext.BuilderSettings.FrameworkServiceProvider.GetServices<IConfigureAppStartupConfigurer<TModule>>())
            {
                configurer.Configure(StartupContext, app);
            }

            ModuleMethodInvoker.CallOptionalMethod(StartupContext.Module, "Configure",
                StartupContext.ServiceProvider,
                app.ApplicationServices,
                app);

        }

        public override IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction)
        {
            var hostBuilderConfigurers = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetServices<IWebModuleWebHostBuilderConfigurer>();

            var moduleAssemblyName = StartupContext.Module.GetType().Assembly.GetName().Name;

#if NETCOREAPP

            var webHostBuilderInitializer = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetService<IWebModuleWebHostBuilderInitializer>();

            if (webHostBuilderInitializer == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var builder = CreateHostBuilder();

            webHostBuilderInitializer.ConfigureWebHost(StartupContext.Module as WebModule, builder,
                new[]
                    {
                        new DelegateWebHostBuilderConfigurer((_, webHostBuilder) =>
                        {
                            webHostBuilder.ConfigureAppConfiguration(
                                (ctx, b) => { ctx.HostingEnvironment.ApplicationName = moduleAssemblyName; });
                        })
                    }.Union(hostBuilderConfigurers)
                    .Append(
                        new DelegateWebHostBuilderConfigurer((_, webHostBuilder) =>
                        {
                            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
                            {
                                webHostBuilder.ConfigureServices(srv => configureServicesAction(StartupContext.BuilderSettings.HostBuilderContext, srv));
                            }

                            webHostBuilder.ConfigureServices(ConfigureServices);
                            webHostBuilder.Configure(Configure);
                        })));

            configureHostBuilderAction?.Invoke(builder);
            
            return builder.Build();
#else

            HostBuilderContext WebContextToHostBuilderContext(WebHostBuilderContext webContext)
            {
                return new HostBuilderContext(new Dictionary<object, object> { { "WebHostBuilderContext", webContext } })
                {
                    Configuration = webContext.Configuration,
                    HostingEnvironment = new HostingEnvironment
                    {
                        ApplicationName = webContext.HostingEnvironment.ApplicationName,
                        ContentRootFileProvider = webContext.HostingEnvironment.ContentRootFileProvider,
                        ContentRootPath = webContext.HostingEnvironment.ContentRootPath,
                        EnvironmentName = webContext.HostingEnvironment.EnvironmentName,
                    }
                };
            }

            var webHostBuilderFactory = StartupContext.BuilderSettings.FrameworkServiceProvider
                .GetService<IWebModuleWebHostBuilderFactory>();

            if (webHostBuilderFactory == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var webHostBuilder = webHostBuilderFactory.CreateWebHost(StartupContext.Module as WebModule);

            webHostBuilder.UseConfiguration(StartupContext.BuilderSettings.HostBuilderContext.Configuration);
            webHostBuilder.UseContentRoot(GetContentRoot());
            webHostBuilder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;
            });

            foreach (var hostBuilderConfigurer in hostBuilderConfigurers)
            {
                hostBuilderConfigurer.ConfigureWebHost(StartupContext.Module as WebModule, webHostBuilder);
            }

            foreach (var configureAction in StartupContext.BuilderSettings.ConfigurationActions)
            {
                webHostBuilder.ConfigureAppConfiguration((webCtx, configure) =>
                    configureAction(WebContextToHostBuilderContext(webCtx), configure));
            }

            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
            {
                webHostBuilder.ConfigureServices(srv =>
                    configureServicesAction(StartupContext.BuilderSettings.HostBuilderContext, srv));
            }

            var webHost = webHostBuilder.ConfigureServices(ConfigureServices)
                .Configure(Configure)
                .Build();

            return new WebHostWrapperHost(webHost);

#endif

        }

    }
}