using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModuleHostBuilder : IModuleHostBuilder
    {

        public IDictionary<object, object> Properties => _innerBuilder.Properties;

        private readonly Dictionary<Type, ModuleHostBootstrapActions> _registeredModules = new Dictionary<Type, ModuleHostBootstrapActions>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureFrameworkActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private bool _hostBuilt;
        private readonly IHostBuilder _innerBuilder = new HostBuilder();
        
        public IModuleHostBuilder HostModule<TModule>(Action<IServiceProvider> bootstrap = null) where TModule : class, IModule
        {
            CheckIfModuleAlreadyRegistered<TModule>();
            _registeredModules.Add(typeof(TModule), new ModuleHostBootstrapActions { Bootstrap = bootstrap});
            return this;
        }

        public IModuleHostBuilder HostModule<TModule>(Action<IHostBuilder> configure, Action<IServiceProvider> bootstrap = null) where TModule : class, IModule
        {
            CheckIfModuleAlreadyRegistered<TModule>();
            _registeredModules.Add(typeof(TModule), new ModuleHostBootstrapActions { ConfigureBuilder = configure, Bootstrap = bootstrap });
            return this;
        }

        private void CheckIfModuleAlreadyRegistered<TModule>() where TModule : IModule
        {
            if (_registeredModules.ContainsKey(typeof(TModule)))
                throw new InvalidOperationException($"Module of type {typeof(TModule)} is already used.");

        }

        public IModuleHostBuilder ConfigureHostConfiguration(
            Action<IConfigurationBuilder> configureDelegate)
        {
            _innerBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        public IModuleHostBuilder ConfigureFrameworkServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureFrameworkActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IModuleHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleConfigurationConfigurer>(sp =>
                    new DelegateModuleHostBuilderConfigurationConfigurer(configureDelegate));
            });

            return this;
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
        }

        public IModuleHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleServicesConfigurer>(sp =>
                    new DelegateModuleServicesConfigurer(configureDelegate));
            });

            return this;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            return UseServiceProviderFactory(factory);
        }

#if NETSTANDARD2_1
       
        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            return UseServiceProviderFactory(factory);
        }
#endif

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            return ConfigureContainer(configureDelegate);
        }

        public IHost Build()
        {
            if (_hostBuilt)
                throw new InvalidOperationException("Build can only be called once.");

            _hostBuilt = true;

            AddModuleContextAccessor();

            IServiceProvider frameworkServices = null;
            object moduleHostServicesFactoryState = null;
            var host = _innerBuilder.ConfigureServices((ctx, services) =>
            {
                frameworkServices = CreateFrameworkServiceProvider(ctx);

                var moduleHostServicesFactory = frameworkServices.GetService<IModuleHostServiceProviderFactory>();
                moduleHostServicesFactoryState = moduleHostServicesFactory?.ConfigureServices(services);

                foreach (var moduleType in _registeredModules.Keys)
                {
                    services.AddSingleton(moduleType);
                }

                RegisterModulesAndHosts(services, frameworkServices);
                
            }).Build();

            var moduleHostServiceProvider = host.Services;

            if (moduleHostServicesFactoryState != null)
            {
                var moduleHostServicesFactory = frameworkServices.GetService<IModuleHostServiceProviderFactory>();
                moduleHostServiceProvider =
                    moduleHostServicesFactory.ReplaceServiceProvider(moduleHostServicesFactoryState,
                        moduleHostServiceProvider);
            }


            //bootstrap internal hosts (setup inner host and containers for modules)
            BootstrapModuleHosts(moduleHostServiceProvider, frameworkServices);

            return host;
        }

        private void RegisterModulesAndHosts(IServiceCollection services, IServiceProvider frameworkServices)
        {
            foreach (var module in _registeredModules)
            {
                var internalHostType = typeof(Internal.IModuleHost<>).MakeGenericType(module.Key);
                var serviceType = typeof(IModuleHost<>).MakeGenericType(module.Key);
                var hostedServiceType = typeof(ModuleHostService<>).MakeGenericType(module.Key);
                
                var internalHost = frameworkServices.GetRequiredService(internalHostType);
                services.AddSingleton(serviceType, sp => internalHost);
                services.AddTransient(typeof(IHostedService), hostedServiceType);
            }
        }

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            return ConfigureHostConfiguration(configureDelegate);
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
        /// <param name="factory">A factory used for creating service providers.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public IModuleHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _innerBuilder.UseServiceProviderFactory(factory);
            return this;
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <param name="factory">A factory used for creating service providers.</param>
        /// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public IModuleHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _innerBuilder.UseServiceProviderFactory(factory);
            return this;
        }
#endif
        private IServiceProvider CreateFrameworkServiceProvider(HostBuilderContext ctx)
        {
            var frameworkServices = new ServiceCollection();

            frameworkServices.AddSingleton(ctx.HostingEnvironment);
            frameworkServices.AddTransient<IHostFactory,DefaultHostFactory>();
            frameworkServices.AddSingleton(ctx);

            _configureFrameworkActions.ForEach(c => c(ctx, frameworkServices));
            
            frameworkServices.TryAddSingleton(typeof(Internal.IModuleHost<>), typeof(ModuleHost<>));
            frameworkServices.TryAddTransient(typeof(IModuleContextFactory<>), typeof(DefaultModuleContextFactory<>));


            return frameworkServices.BuildServiceProvider(true);

        }

        private void AddModuleContextAccessor()
        {
            ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IModuleContextAccessor, ModuleContextAccessor>();
            });
        }
        
        public IModuleHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _innerBuilder.ConfigureContainer(configureDelegate);
            return this;
        }


        private static IModuleHost GetModuleHost(Type moduleType, IServiceProvider frameworkServices)
        {
            var hostType = typeof(Internal.IModuleHost<>).MakeGenericType(moduleType);
            var internalHost = frameworkServices.GetRequiredService(hostType) as IModuleHost;
            return internalHost;
        }

        private void BootstrapModuleHosts(IServiceProvider moduleHostServices, IServiceProvider frameworkServices)
        {
            foreach (var module in _registeredModules)
            {
                GetModuleHost(module.Key, frameworkServices)?.Bootstrap(moduleHostServices, module.Value);
            }
        }


    }
}
