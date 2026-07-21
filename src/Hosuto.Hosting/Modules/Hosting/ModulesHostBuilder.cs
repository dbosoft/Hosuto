using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModulesHostBuilder : IModulesHostBuilder
    {

        public IDictionary<object, object> Properties => _innerBuilder.Properties;

        private readonly Dictionary<Type, IModuleRegistration> _registeredModules = new Dictionary<Type, IModuleRegistration>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureFrameworkActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private bool _hostBuilt;
        private readonly IHostBuilder _innerBuilder = new HostBuilder();
        
        public IModulesHostBuilder HostModule<TModule>(Action<IModuleHostingOptions> options = null) where TModule : class
        {
            // Generic overload: the closed ModuleRegistration<TModule> keeps the module type as a
            // static generic parameter, so no reflection/MakeGenericType is needed to host it.
            AddModuleRegistration(new ModuleRegistration<TModule>(CreateHostingOptions(options)));
            return this;
        }

        public IModulesHostBuilder HostModule(Type moduleType, Action<IModuleHostingOptions> options = null)
        {
            if (moduleType == null) throw new ArgumentNullException(nameof(moduleType));

            // Type-based overload: the module type is only known at runtime, so the closed
            // registration has to be created reflectively here. Prefer the generic HostModule<TModule>
            // overload, which is trimming/AOT friendly.
            var registration = (IModuleRegistration)Activator.CreateInstance(
                typeof(ModuleRegistration<>).MakeGenericType(moduleType), CreateHostingOptions(options));

            AddModuleRegistration(registration);
            return this;
        }

        private void AddModuleRegistration(IModuleRegistration registration)
        {
            if (_registeredModules.ContainsKey(registration.ModuleType))
                throw new InvalidOperationException($"Module of type {registration.ModuleType} is already used.");

            _registeredModules.Add(registration.ModuleType, registration);
        }

        private static ModuleHostingOptions CreateHostingOptions(Action<IModuleHostingOptions> options)
        {
            var hostingOptions = new ModuleHostingOptions();
            options?.Invoke(hostingOptions);
            return hostingOptions;
        }

        public IModulesHostBuilder ConfigureHostConfiguration(
            Action<IConfigurationBuilder> configureDelegate)
        {
            _innerBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        public IModulesHostBuilder ConfigureInternalHost(Action<IHostBuilder> configureDelegate)
        {
            configureDelegate(_innerBuilder);
            return this;
        }

        public IModulesHostBuilder ConfigureFrameworkServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureFrameworkActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IModulesHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _innerBuilder.ConfigureAppConfiguration(configureDelegate);

            ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleConfigurationFilter>(sp =>
                    new DelegateModuleHostBuilderConfigurationConfigurer(configureDelegate));
            });

            return this;
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
        }

        public IModulesHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _innerBuilder.ConfigureServices(configureDelegate);

            ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleServicesFilter>(sp =>
                    new DelegateModuleServicesFilter(configureDelegate));
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

                foreach (var registration in _registeredModules.Values)
                {
                    registration.RegisterModule(services, moduleHostServicesFactory);
                    registration.RegisterHost(services, frameworkServices);
                }

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
        public IModulesHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
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
        public IModulesHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _innerBuilder.UseServiceProviderFactory(factory);
            return this;
        }
#endif
        private IServiceProvider CreateFrameworkServiceProvider(HostBuilderContext ctx)
        {
            var frameworkServices = new ServiceCollection();

            frameworkServices.AddSingleton(ctx.HostingEnvironment);
            frameworkServices.AddSingleton(ctx);

            _configureFrameworkActions.ForEach(c => c(ctx, frameworkServices));
            
            frameworkServices.TryAddSingleton(typeof(Internal.IModuleHost<>), typeof(ModuleHost<>));
            frameworkServices.TryAddTransient(typeof(IModuleContextFactory<>), typeof(DefaultModuleContextFactory<>));


            return frameworkServices.BuildServiceProvider(true);

        }

        private void AddModuleContextAccessor()
        {
            ((IHostBuilder)this).ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IModuleContextAccessor, ModuleContextAccessor>();
            });
        }
        
        public IModulesHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _innerBuilder.ConfigureContainer(configureDelegate);
            return this;
        }


        private void BootstrapModuleHosts(IServiceProvider moduleHostServices, IServiceProvider frameworkServices)
        {
            foreach (var registration in _registeredModules.Values)
            {
                registration.Bootstrap(moduleHostServices, frameworkServices);
            }
        }


    }
}
