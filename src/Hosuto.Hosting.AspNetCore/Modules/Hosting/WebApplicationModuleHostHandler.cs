#if NET6_0_OR_GREATER
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    /// <summary>
    /// Builds a web module's inner host as a minimal-API <see cref="WebApplication"/> instead of the
    /// classic <c>HostBuilder + ConfigureWebHostDefaults</c> host. Opt-in via
    /// <c>UseAspNetCoreMinimal()</c>. The module's <c>ConfigureServices</c> runs against
    /// <see cref="WebApplicationBuilder.Services"/> before build; <c>Configure</c> is deferred to
    /// pipeline build (start) via an <see cref="IStartupFilter"/> so it runs after the module
    /// container (SimpleInjector/Autofac) has been configured by the bootstrap pipeline.
    /// </summary>
    public class WebApplicationModuleHostHandler<TModule> : DefaultBootstrapHostHandler<TModule> where TModule : class
    {
        public override void BootstrapHost(BootstrapModuleHostCommand<TModule> command)
        {
            if (command.Host != null || !(command.BootstrapContext.Module is IWebModule))
                return;

            BootstrapContext = command.BootstrapContext;

            var frameworkServices = BootstrapContext.Advanced.FrameworkServices;
            var hostBuilderContext = frameworkServices.GetRequiredService<HostBuilderContext>();
            var moduleAssemblyName = BootstrapContext.Module.GetType().Assembly.GetName().Name;

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = moduleAssemblyName,
                EnvironmentName = hostBuilderContext.HostingEnvironment.EnvironmentName,
                ContentRootPath = hostBuilderContext.HostingEnvironment.ContentRootPath
            });

            // configuration: outer app configuration + module configuration filters.
            builder.Configuration.AddConfiguration(hostBuilderContext.Configuration);
            Filters.BuildFilterPipeline(
                frameworkServices.GetServices<IModuleConfigurationFilter>(),
                (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(hostBuilderContext), builder.Configuration);

            // services: reuse the base pipeline (module services filters + module ConfigureServices,
            // interface or convention).
            // ReSharper disable once ConvertToUsingDeclaration
            using (var tempServiceProvider = builder.Services.BuildServiceProvider())
            {
                ConfigureServices(hostBuilderContext, builder.Services, tempServiceProvider);
            }

            // TODO: module static web assets (wwwroot / _content, namespaced as ".modules/{module}")
            // are not yet mapped for the minimal-API host. The classic ModuleWebAssetsLoader is
            // file-provider/XML-manifest based and does not fit the endpoint-based static web assets
            // introduced in .NET 9. Tracked as a follow-up (see samples/dotnet/minimal/README.md).

            // let opt-in configurers touch the WebApplicationBuilder (e.g. Kestrel/urls).
            foreach (var configurer in frameworkServices.GetServices<IWebApplicationBuilderConfigurer>())
                configurer.Configure(builder);

            // defer module Configure to pipeline build (start), after the container is configured.
            builder.Services.AddSingleton<IStartupFilter>(new ModuleConfigureStartupFilter(app => ConfigureApp(command, app)));

            command.Options.ConfigureBuilderAction?.Invoke(builder.Host);

            command.Host = builder.Build();
            command.ModuleContext = CreateModuleContext(command.Host.Services);
        }

        private void ConfigureApp(BootstrapModuleHostCommand<TModule> command, IApplicationBuilder app)
        {
            Filters.BuildFilterPipeline(
                BootstrapContext.Advanced.FrameworkServices.GetServices<IWebModuleConfigureFilter>(),
                (ctx, appBuilder) =>
                {
                    var innerContext = BootstrapContext.ToModuleContext(app.ApplicationServices);
                    if (innerContext.Module is IApplicationConfiguringModule configuringModule)
                        configuringModule.Configure(innerContext.ModulesHostServices, appBuilder);
                    else
                        ModuleMethodInvoker.CallOptionalMethod(innerContext, "Configure", appBuilder);
                })(command.ModuleContext, app);
        }
    }
}
#endif
