using System;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
#if !NETCOREAPP
using Microsoft.AspNetCore.Hosting.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using HostingEnvironment = Microsoft.Extensions.Hosting.Internal.HostingEnvironment;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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


        public (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, ModuleHostingOptions options) where TModule : IModule
        {
            switch (bootstrapContext.Module)
            {
                case WebModule _:
                {
                    var factory = new WebModuleHostFactory<TModule>(bootstrapContext);
                    return factory.CreateHost(options);
                }

                default:
                    return _decoratedHostFactory.CreateHost(bootstrapContext, options);
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
            Filters.BuildFilterPipeline(
                BootstrapContext.Advanced.FrameworkServices.GetServices<IWebModuleConfigureFilter>(),
                (ctx, appBuilder) =>
                {
                    ModuleMethodInvoker.CallOptionalMethod(BootstrapContext.ToModuleContext(app.ApplicationServices), "Configure", appBuilder);

                })(moduleContext, app);

        }
        
        public override (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost(ModuleHostingOptions options)
        {
            var webHostBuilderFilters = BootstrapContext.Advanced.FrameworkServices
                .GetServices<IWebModuleWebHostBuilderFilter>();

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
                        new DelegateWebModuleWebHostBuilderFilter((_, webHostBuilder) =>
                        {
                            webHostBuilder.ConfigureAppConfiguration(
                                (ctx, b) => { ctx.HostingEnvironment.ApplicationName = moduleAssemblyName; });
                        })
                    }.Union(webHostBuilderFilters)
                    .Append(
                        new DelegateWebModuleWebHostBuilderFilter((_, webHostBuilder) =>
                        {
                            webHostBuilder.Configure(app =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                Configure(moduleContext, app);
                            });
                        })));

            options.ConfigureBuilderAction?.Invoke(builder);

            var host = builder.Build();
            moduleContext = CreateModuleContext(host.Services);
            return (host, moduleContext);

#else
            if(options.ConfigureBuilderAction != null)
                throw new InvalidOperationException("The configure method with IHostBuilder is not supported for ASPNETCORE < 3.0. You should configure the webHostBuilder instead.");

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

                Filters.BuildFilterPipeline(
                    BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationFilter>(),
                    (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(WebContextToHostBuilderContext(ctx)), config);
                
            });

            Filters.BuildFilterPipeline(webHostBuilderFilters, (_, __) => { })(BootstrapContext.Module as WebModule, webHostBuilder);


            webHostBuilder.ConfigureServices((webContext, services) =>
            {
                var tempServiceProvider = services.BuildServiceProvider();

                Action<IServiceCollection> configureMethod = (s) =>
                {
                    ConfigureServices(WebContextToHostBuilderContext(webContext), s, tempServiceProvider);

                };

                configureMethod = BuildConfigureServicesFilterPipeline(tempServiceProvider, configureMethod);
                configureMethod(services);
            });

            webHostBuilder.Configure(app =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    Configure(moduleContext, app);

                });

            options.ConfigureWebHostBuilder(webHostBuilder);
            var webHost = options.BuildWebHost(webHostBuilder);

            moduleContext = CreateModuleContext(webHost.Services);

            return (new WebHostWrapperHost(webHost), moduleContext);

#endif

        }

#if NETSTANDARD
        private static HostBuilderContext WebContextToHostBuilderContext(WebHostBuilderContext webContext)
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
        
        private static Action<IServiceCollection> BuildConfigureServicesFilterPipeline(IServiceProvider serviceProvider,  Action<IServiceCollection> configureServices)
        {
            return (services =>
            {
#pragma warning disable CS0612 // Type or member is obsolete
            var filters = serviceProvider.GetRequiredService<IEnumerable<IStartupConfigureServicesFilter>>().Reverse().ToArray();
#pragma warning restore CS0612 // Type or member is obsolete

            if (filters.Length == 0)
            {
                configureServices(services);
                return;
            }

            var pipeline = filters.Aggregate(configureServices,
                (current, t) => t.ConfigureServices(current));

            pipeline(services);

            });

        }


#endif
    }
}