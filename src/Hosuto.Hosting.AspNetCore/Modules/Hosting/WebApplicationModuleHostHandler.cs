#if NET6_0_OR_GREATER
using System.Collections.Generic;
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
    /// container (e.g. SimpleInjector) has been configured by the bootstrap pipeline.
    /// </summary>
    /// <remarks>
    /// A module hosting options <c>Configure(Action&lt;IHostBuilder&gt;)</c> delegate is applied to
    /// <see cref="WebApplicationBuilder.Host"/>, which restricts some <see cref="IHostBuilder"/>
    /// operations (it manages the service provider factory itself).
    /// </remarks>
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

            // configuration: outer app configuration + module application name + module config filters.
            builder.Configuration.AddConfiguration(hostBuilderContext.Configuration);
            builder.Configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>(HostDefaults.ApplicationKey, moduleAssemblyName)
            });

            // a HostBuilderContext that reflects the *inner* module host (its environment and
            // configuration), so module configuration/services filters see the module host being
            // built rather than the outer host.
            var moduleHostContext = new HostBuilderContext(new Dictionary<object, object>())
            {
                HostingEnvironment = builder.Environment,
                Configuration = builder.Configuration
            };

            Filters.BuildFilterPipeline(
                frameworkServices.GetServices<IModuleConfigurationFilter>(),
                (_, __) => { })(BootstrapContext.ToModuleHostBuilderContext(moduleHostContext), builder.Configuration);

            // services: reuse the base pipeline (module services filters + module ConfigureServices,
            // interface or convention).
            // ReSharper disable once ConvertToUsingDeclaration
            using (var tempServiceProvider = builder.Services.BuildServiceProvider())
            {
                ConfigureServices(moduleHostContext, builder.Services, tempServiceProvider);
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

            // honor the module's service-provider validation override (parity with the classic host;
            // see IModuleHostingOptions.ValidateServiceProvider). Without this the .NET 9+ Development
            // default (ValidateOnBuild=true) can fail modules whose dependencies live in the container.
            // NB: must be builder.Host (not builder.WebHost) - WebApplicationBuilder ignores the
            // WebHost's service-provider factory but honors the Host's.
            if (command.Options.HasServiceProviderValidationOverride())
                builder.Host.UseDefaultServiceProvider((_, options) =>
                    ApplyServiceProviderValidation(options, command.Options));

            command.Options.ConfigureBuilderAction?.Invoke(builder.Host);

            command.Host = builder.Build();
            command.ModuleContext = CreateModuleContext(command.Host.Services);
        }

        private void ConfigureApp(BootstrapModuleHostCommand<TModule> command, IApplicationBuilder app)
        {
            // middleware: the module's Configure (interface or convention).
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

            // endpoints: minimal-API-style mapping via IEndpointConfiguringModule.
            var endpointContext = BootstrapContext.ToModuleContext(app.ApplicationServices);
            if (endpointContext.Module is IEndpointConfiguringModule endpointModule)
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                    endpointModule.MapEndpoints(endpointContext.ModulesHostServices, endpoints));
            }
        }
    }
}
#endif
