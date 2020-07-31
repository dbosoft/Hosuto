using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModuleHostBuilder : IModuleHostBuilder
    {

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        private readonly Dictionary<Type, Action<IHostBuilder>> _registeredModules = new Dictionary<Type, Action<IHostBuilder>>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureFrameworkActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();

        private IServiceFactoryAdapter _serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());

        private bool _hostBuilt;
        private IConfiguration _hostConfiguration;

        private HostBuilderContext _hostBuilderContext;
        private HostingEnvironment _hostingEnvironment;

        public IModuleHostBuilder HostModule<TModule>(Action<IHostBuilder> configureDelegate = null) where TModule : class, IModule
        {
            if(_registeredModules.ContainsKey(typeof(TModule)))
                throw new InvalidOperationException($"Module of type {typeof(TModule)} is already used.");

            _registeredModules.Add(typeof(TModule), configureDelegate);
            return this;
        }

        public IModuleHostBuilder ConfigureHostConfiguration(
            Action<IConfigurationBuilder> configureDelegate)
        {
            _configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
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

        public IModuleHost Build()
        {
            if (_hostBuilt)
                throw new InvalidOperationException("Build can only be called once.");

            _hostBuilt = true;

            BuildHostConfiguration();
            CreateHostingEnvironment();
            CreateHostBuilderContext();


            //build the services for configuration of Hosuto module system
            var frameworkServices = CreateFrameworkServiceProvider();

            //create the service provider for the module host
            var moduleHostServices = CreateServiceProvider(frameworkServices);

            //bootstrap internal hosts (setup inner host and containers for modules)
            BootstrapHosts(moduleHostServices, frameworkServices);
            
            //wrap the internal generic or collection module host in a ModulHost
            return new ModuleHost(MergeInternalHosts(frameworkServices),moduleHostServices);

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
            _serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <param name="factory">A factory used for creating service providers.</param>
        /// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public IModuleHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => _hostBuilderContext, factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        private void BuildHostConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection();
            _configureHostConfigActions.ForEach(c=>c(configurationBuilder));
            _hostConfiguration = configurationBuilder.Build();
        }

        private void CreateHostingEnvironment()
        {
            _hostingEnvironment = new HostingEnvironment()
            {
                ApplicationName = _hostConfiguration[HostDefaults.ApplicationKey],
                EnvironmentName = _hostConfiguration[HostDefaults.EnvironmentKey] ??
#if NETSTANDARD2_1
                                  Environments.Production,
#else
                                  EnvironmentName.Production,
#endif
                ContentRootPath = ResolveContentRootPath(_hostConfiguration[HostDefaults.ContentRootKey], AppContext.BaseDirectory)
            };

            if (string.IsNullOrEmpty(_hostingEnvironment.ApplicationName))
            {
                var hostingEnvironment = _hostingEnvironment;
                var entryAssembly = Assembly.GetEntryAssembly();
                hostingEnvironment.ApplicationName = entryAssembly?.GetName().Name;
            }

            _hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath);
        }

        private IServiceProvider CreateFrameworkServiceProvider()
        {
            var frameworkServices = new ServiceCollection();

#if NETSTANDARD2_0
            frameworkServices.AddSingleton((IHostingEnvironment)_hostingEnvironment);
#else
            frameworkServices.AddSingleton((IHostEnvironment)_hostingEnvironment);

#endif
            frameworkServices.AddSingleton(_hostBuilderContext);
            frameworkServices.AddTransient<IHostFactory,DefaultHostFactory>();

            _configureFrameworkActions.ForEach(c => c(_hostBuilderContext, frameworkServices));
            
            frameworkServices.TryAddSingleton(typeof(IInternalModuleHost<>), typeof(GenericModuleHost<>));
            frameworkServices.TryAddTransient(typeof(IModuleContextFactory<>), typeof(DefaultModuleContextFactory<>));


            return frameworkServices.BuildServiceProvider(true);

        }

        private IServiceProvider CreateServiceProvider(IServiceProvider frameworkServices)
        {
            var moduleServices = new ServiceCollection()
#if NETSTANDARD2_0
                .AddSingleton((IHostingEnvironment) _hostingEnvironment)
#else
                 .AddSingleton((IHostEnvironment) _hostingEnvironment)
#endif
                .AddSingleton(_hostBuilderContext.Configuration);

            foreach (var moduleType in _registeredModules.Keys)
            {
                moduleServices.AddSingleton(moduleType);
            }

            WireModuleHosts(moduleServices, frameworkServices);


            var containerBuilder = _serviceProviderFactory.CreateBuilder(moduleServices);
            return _serviceProviderFactory.CreateServiceProvider(containerBuilder);

        }

        private IInternalModuleHost GetModuleHost(Type moduleType, IServiceProvider frameworkServices) 
        {
            var hostType = typeof(IInternalModuleHost<>).MakeGenericType(moduleType);
            var internalHost = frameworkServices.GetRequiredService(hostType) as IInternalModuleHost;
            return internalHost;
        }

        private void BootstrapHosts(IServiceProvider moduleHostServices, IServiceProvider frameworkServices)
        {
            foreach (var module in _registeredModules)
            {
                GetModuleHost(module.Key, frameworkServices)?.Bootstrap(moduleHostServices);
            }
        }

        private void WireModuleHosts(IServiceCollection moduleHostServices, IServiceProvider frameworkServices)
        {
            foreach (var module in _registeredModules)
            {
                var hostType = typeof(IInternalModuleHost<>).MakeGenericType(module.Key);
                var serviceType = typeof(IModuleHost<>).MakeGenericType(module.Key);

                var internalHost = frameworkServices.GetRequiredService(hostType);
                moduleHostServices.AddSingleton(serviceType, sp => internalHost);
            }
        }

        private IModuleHost MergeInternalHosts(IServiceProvider frameworkServices)
        {
            switch (_registeredModules.Count)
            {
                case 0: throw new InvalidOperationException("No modules have been registered in ModuleHost. Use HostModule<> method to add a module to the module host builder.");
                case 1: return GetModuleHost(_registeredModules.First().Key, frameworkServices);
                default: return new ModuleCollectionHost(_registeredModules.Select(
                    m => GetModuleHost(m.Key, frameworkServices)));
            }
        }

        private void CreateHostBuilderContext()
        {
            _hostBuilderContext = new HostBuilderContext(Properties)
            {
                HostingEnvironment = _hostingEnvironment,
                Configuration = _hostConfiguration
            };
        }


        private static string ResolveContentRootPath(string contentRootPath, string basePath)
        {
            if (string.IsNullOrEmpty(contentRootPath))
                return basePath;
            return Path.IsPathRooted(contentRootPath) ? contentRootPath : Path.Combine(Path.GetFullPath(basePath), contentRootPath);
        }


        public IModuleHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            throw new NotImplementedException();
        }

        IHost IHostBuilder.Build()
        {
            return Build();
        }

    }
}
