using System;
using System.Collections.Generic;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using HostingEnvironment = Microsoft.Extensions.Hosting.Internal.HostingEnvironment;
#if !NETCOREAPP
using Microsoft.AspNetCore.Hosting.Internal;
using System.IO;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class WebModuleBootstrapHostHandler<TModule> : DefaultBootstrapHostHandler<TModule> where TModule : class
    {
        protected virtual void Configure(IModuleContext<TModule> moduleContext, IApplicationBuilder app)
        {
            Filters.BuildFilterPipeline(
                BootstrapContext.Advanced.FrameworkServices.GetServices<IWebModuleConfigureFilter>(),
                (ctx, appBuilder) =>
                {
                    ModuleMethodInvoker.CallOptionalMethod(BootstrapContext.ToModuleContext(app.ApplicationServices), "Configure", appBuilder);

                })(moduleContext, app);

        }

        public override void BootstrapHost(BootstrapModuleHostCommand<TModule> command)
        {
            if (command.Host != null  || !(command.BootstrapContext.Module is IWebModule))
                return;

            BootstrapContext = command.BootstrapContext;

            var webHostBuilderFilters = BootstrapContext.Advanced.FrameworkServices
                .GetServices<IWebModuleWebHostBuilderFilter>();

            var moduleAssemblyName = BootstrapContext.Module.GetType().Assembly.GetName().Name;


#if NETCOREAPP

            var webHostBuilderInitializer = BootstrapContext.Advanced.FrameworkServices
                .GetService<IWebModuleWebHostBuilderInitializer>();

            if (webHostBuilderInitializer == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var builder = CreateHostBuilder();

            webHostBuilderInitializer.ConfigureWebHost(BootstrapContext.Module as IWebModule, builder,
                new[]
                    {
                        new DelegateWebModuleWebHostBuilderFilter((_, webHostBuilder) =>
                        {
                            webHostBuilder.ConfigureAppConfiguration(
                                (ctx, b) =>
                                {
                                    ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;
                                });

                            webHostBuilder.ConfigureServices((ctx, services) =>
                            {
                                // ReSharper disable once ConvertToUsingDeclaration
                                using(var tempServiceProvider = services.BuildServiceProvider())
                                {
                                    ConfigureServices(WebContextToHostBuilderContext(ctx), services, tempServiceProvider);
                                }
                            });

                            
                        })
                    }.Union(webHostBuilderFilters)
                    .Append(
                        new DelegateWebModuleWebHostBuilderFilter((_, webHostBuilder) =>
                        {
                            AddModuleContent(webHostBuilder);

                            webHostBuilder.Configure(app =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                Configure(command.ModuleContext, app);

                            });
                            

                        })));
            ApplyConfiguration(builder);

            command.Options.ConfigureBuilderAction?.Invoke(builder);

            command.Host = builder.Build();
            command.ModuleContext = CreateModuleContext(command.Host.Services);

#else
            if(command.Options.ConfigureBuilderAction != null)
                throw new InvalidOperationException("The configure method with IHostBuilder is not supported for ASPNETCORE < 3.0. You should configure the webHostBuilder instead.");

            var webHostBuilderFactory = BootstrapContext.Advanced.FrameworkServices
                .GetService<IWebModuleWebHostBuilderFactory>();

            if (webHostBuilderFactory == null)
                throw new InvalidOperationException("AspNetCore runtime not configured.");


            var webHostBuilder = webHostBuilderFactory.CreateWebHost(BootstrapContext.Module as IWebModule);
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();
            webHostBuilder.UseConfiguration(hostBuilderContext.Configuration);
            
            if(hostBuilderContext.HostingEnvironment.IsDevelopment())
                webHostBuilder.UseContentRoot(GetRelativeModulePath());

            webHostBuilder.ConfigureAppConfiguration((ctx, config) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;

                Filters.BuildFilterPipeline(
                    BootstrapContext.Advanced.FrameworkServices.GetServices<IModuleConfigurationFilter>(),
                    (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(WebContextToHostBuilderContext(ctx)), config);
                
            });

            Filters.BuildFilterPipeline(webHostBuilderFilters, (_, __) => { })(BootstrapContext.Module as IWebModule, webHostBuilder);


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
                    Configure(command.ModuleContext, app);

                });

            command.Options.ConfigureWebHostBuilder(webHostBuilder);
            var webHost = command.Options.BuildWebHost(webHostBuilder);

            command.ModuleContext = CreateModuleContext(webHost.Services);
            command.Host = new WebHostWrapperHost(webHost);

#endif

        }

#if NETCOREAPP
        private void AddModuleContent(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureAppConfiguration((ctx,config) =>
            {

                ModuleWebAssets.ModuleWebAssetsLoader
                    .UseModuleAssets(ctx.HostingEnvironment, ctx.Configuration);
                
                if (!ctx.HostingEnvironment.IsDevelopment()) return;

                var realHostEnv = BootstrapContext.ModulesHostServices.GetRequiredService<IHostEnvironment>();

                ModuleWebAssets.ModuleWebAssetsLoader
                    .UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration, realHostEnv.ApplicationName);
            });
        }
#endif

#if NETSTANDARD
        protected virtual string GetRelativeModulePath()
        {
            var hostBuilderContext = BootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();

            var name = "";
            if (BootstrapContext.Module is INamedModule namedModule)
                name = namedModule.Name;

            name = name ?? BootstrapContext.Module.GetType().Assembly.GetName().Name;

            var pathCandidate = Path.Combine(hostBuilderContext.HostingEnvironment
                .ContentRootPath, "..", name);

            if (!Directory.Exists(pathCandidate))
                return hostBuilderContext.HostingEnvironment.ContentRootPath;


            return Path.GetFullPath(pathCandidate);

        }
#endif

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
#if NETSTANDARD
        private static Action<IServiceCollection> BuildConfigureServicesFilterPipeline(IServiceProvider serviceProvider,  Action<IServiceCollection> configureServices)
        {
            return (services =>
            {
#pragma warning disable CS0612 // Type or member is obsolete
            var filters = serviceProvider.GetRequiredService<IEnumerable<IStartupConfigureServicesFilter>>().ToArray();
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