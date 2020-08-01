using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
#if NETCOREAPP
using System.Linq;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;


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


        public (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, Action<IHostBuilder> configure) where TModule : IModule
        {
            switch (bootstrapContext.Module)
            {
                case WebModule _:
                {
                    var factory = new WebModuleHostFactory<TModule>(bootstrapContext);
                    return factory.CreateHost(configure);
                }

                default:
                    return _decoratedHostFactory.CreateHost(bootstrapContext, configure);
            }
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class WebModuleHostFactory<TModule> : DefaultHostFactory<TModule> where TModule : IModule
    {
        public WebModuleHostFactory(IModuleBootstrapContext<TModule> bootstrapContext) : base(bootstrapContext)
        {
        }

        protected virtual void Configure(IModuleContext<TModule> moduleContext, IApplicationBuilder app)
        {
            foreach (var configurer in BootstrapContext.Advanced.FrameworkServices.GetServices<IConfigureAppConfigurer<TModule>>())
            {
                configurer.Configure(moduleContext, app);
            }

            ModuleMethodInvoker.CallOptionalMethod(BootstrapContext, "Configure", app.ApplicationServices, app);

        }

        public override (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost(Action<IHostBuilder> configure)
        {
            var hostBuilderConfigurers = BootstrapContext.Advanced.FrameworkServices
                .GetServices<IWebModuleWebHostBuilderConfigurer>();

            var moduleAssemblyName = BootstrapContext.Module.GetType().Assembly.GetName().Name;
            IModuleContext<TModule> moduleContext = null;


#if NETCOREAPP

            var webHostBuilderInitializer = BootstrapContext.Advanced.FrameworkServices
                .GetService<IWebModuleWebHostBuilderInitializer>();

            if (webHostBuilderInitializer == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var builder = CreateHostBuilder();
            
            webHostBuilderInitializer.ConfigureWebHost(BootstrapContext.Module as WebModule, builder,
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
                            webHostBuilder.ConfigureServices((wctx, services)=> 
                                ConfigureServices(WebContextToHostBuilderContext(wctx), services));

                            webHostBuilder.Configure(app =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                Configure(moduleContext, app);
                            });
                        })));

            configure?.Invoke(builder);

            var host = builder.Build();
            moduleContext = CreateModuleContext(host.Services);
            return (host, moduleContext);

#else
            if(configure!=null)
                throw new InvalidOperationException("The configure delegate is not supported for ASPNETCORE < 3.0. You should configure the webHostBuilder instead.");

            var webHostBuilderFactory = BootstrapContext.Advanced.FrameworkServices
                .GetService<IWebModuleWebHostBuilderFactory>();

            if (webHostBuilderFactory == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var webHostBuilder = webHostBuilderFactory.CreateWebHost(BootstrapContext.Module as WebModule);
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();
            webHostBuilder.UseConfiguration(hostBuilderContext.Configuration);
            webHostBuilder.UseContentRoot(GetContentRoot());
            webHostBuilder.ConfigureAppConfiguration((ctx, config) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;

                foreach (var configurer in BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationConfigurer>())
                {
                    configurer.ConfigureModuleConfiguration(
                        BootstrapContext.ToModuleHostBuilderContext(WebContextToHostBuilderContext(ctx)), config);
                }

            });

            foreach (var hostBuilderConfigurer in hostBuilderConfigurers)
            {
                hostBuilderConfigurer.ConfigureWebHost(BootstrapContext.Module as WebModule, webHostBuilder);
            }

            var webHost = webHostBuilder.ConfigureServices((wctx, services) =>
                    ConfigureServices(WebContextToHostBuilderContext(wctx), services))
                .Configure(app =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    Configure(moduleContext, app);

                })
                .Build();

            moduleContext = CreateModuleContext(webHost.Services);

            return (new WebHostWrapperHost(webHost), moduleContext);

#endif

        }

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

    }
}