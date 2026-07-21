using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    /// <summary>
    /// Non-generic handle for a registered module. Concrete instances are
    /// <see cref="ModuleRegistration{TModule}"/>, which keeps the module type as a static generic
    /// parameter so all closed generic instantiations (module host, hosted service, module type) are
    /// visible to the (AOT) compiler instead of being reconstructed at runtime via reflection.
    /// </summary>
    internal interface IModuleRegistration
    {
        Type ModuleType { get; }

        ModuleHostingOptions Options { get; }

        void RegisterModule(IServiceCollection services, IModuleHostServiceProviderFactory serviceProviderFactory);

        void RegisterHost(IServiceCollection services, IServiceProvider frameworkServices);

        void Bootstrap(IServiceProvider moduleHostServices, IServiceProvider frameworkServices);
    }

    internal sealed class ModuleRegistration<TModule> : IModuleRegistration where TModule : class
    {
        public ModuleRegistration(ModuleHostingOptions options)
        {
            Options = options;
        }

        public Type ModuleType => typeof(TModule);

        public ModuleHostingOptions Options { get; }

        public void RegisterModule(IServiceCollection services, IModuleHostServiceProviderFactory serviceProviderFactory)
        {
            if (serviceProviderFactory != null)
            {
                serviceProviderFactory.ConfigureModule(typeof(TModule), Options.ModuleFactory);
                return;
            }

            if (Options.ModuleFactory == null)
                services.AddSingleton<TModule>();
            else
                services.AddSingleton(sp => (TModule)Options.ModuleFactory(sp));
        }

        public void RegisterHost(IServiceCollection services, IServiceProvider frameworkServices)
        {
            var internalHost = frameworkServices.GetRequiredService<IModuleHost<TModule>>();

            services.AddSingleton<Dbosoft.Hosuto.Modules.Hosting.IModuleHost<TModule>>(_ => internalHost);
            services.AddTransient<IHostedService, ModulesHostService<TModule>>();
        }

        public void Bootstrap(IServiceProvider moduleHostServices, IServiceProvider frameworkServices)
        {
            frameworkServices.GetRequiredService<IModuleHost<TModule>>()
                .Bootstrap(moduleHostServices, Options);
        }
    }
}
