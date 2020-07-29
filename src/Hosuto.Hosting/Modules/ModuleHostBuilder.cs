using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Dbosoft.Hosuto.Modules
{
    public class ModuleHostBuilder : IModuleHostBuilder
    {

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        private readonly Dictionary<Type, Action<IHostBuilder>> _registeredModules = new Dictionary<Type, Action<IHostBuilder>>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureFrameworkActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();

        private readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
        private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureModulesConfigActions = new List<Action<HostBuilderContext,IConfigurationBuilder>>();
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

        public IModuleHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureModulesConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IModuleHostBuilder ConfigureFrameworkServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureFrameworkActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IModuleHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
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
            var frameworkServiceProvider = CreateFrameworkServiceProvider();
            var moduleHostFactory = frameworkServiceProvider.GetRequiredService<IModuleHostFactory>();

            return moduleHostFactory.CreateModuleHost(
                _registeredModules, 
                new ModuleHostBuilderSettings(_hostBuilderContext, _configureModulesConfigActions, _configureServicesActions, frameworkServiceProvider), 
               CreateServiceProvider());
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
            _configureFrameworkActions.ForEach(c => c(_hostBuilderContext, frameworkServices));

            frameworkServices.TryAddSingleton<IModuleHostFactory, DefaultModuleHostFactory>();
            frameworkServices.TryAddSingleton(typeof(IModuleHost<>), typeof(ModuleHost<>));

            return  frameworkServices.BuildServiceProvider(true);

        }

        private IServiceProvider CreateServiceProvider()
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

            var containerBuilder = _serviceProviderFactory.CreateBuilder(moduleServices);
            return _serviceProviderFactory.CreateServiceProvider(containerBuilder);

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