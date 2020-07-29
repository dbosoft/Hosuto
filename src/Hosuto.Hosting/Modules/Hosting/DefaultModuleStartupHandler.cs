using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultModuleStartupHandler<TModule> : IModuleStartupHandler where TModule : IModule
    {

        public DefaultModuleStartupHandler(ModuleStartupContext<TModule> startupContext)
        {
            StartupContext = startupContext;
        }

        public ModuleStartupContext<TModule> StartupContext { get; }
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            var tempProvider = services.BuildServiceProvider();
            CallOptionalMethod(StartupContext.Module, "ConfigureServices", StartupContext.ServiceProvider, tempProvider, services);

            var configurer = StartupContext.BuilderSettings.FrameworkServiceProvider.GetService<IServicesConfigurer<TModule>>();
            configurer?.ConfigureServices(StartupContext.Module, StartupContext.ServiceProvider, services);
        }

        protected void CallOptionalMethod(IModule module, string methodName, IServiceProvider outerServiceProvider, IServiceProvider serviceProvider, params object[] arguments)
        {
            var moduleType = module.GetType();
            var methodInfo = moduleType.GetMethod(methodName);

            if (methodInfo == null)
                return;

            var parameters = new List<object>();
            for (var index = 0; index < methodInfo.GetParameters().Length; index++)
            {
                var parameter = methodInfo.GetParameters()[index];

                //by convention: when the first argument is a IServiceProvider it is the outer container
                if (index == 0 && parameter.ParameterType == typeof(IServiceProvider))
                {
                    parameters.Add(outerServiceProvider);
                    continue;
                }

                var parameterFound = false;

                foreach (var argument in arguments)
                {
                    if (!parameter.ParameterType.IsInstanceOfType(argument)) continue;

                    parameters.Add(argument);
                    parameterFound = true;
                    break;
                }

                if (parameterFound)
                    continue;

                //resolve all other services from serviceProvider
                var resolvedArgument = serviceProvider.GetService(parameter.ParameterType);
                if (resolvedArgument == null)
                    throw new InvalidOperationException(
                        $"Failed to resolve type {parameter.ParameterType} of argument {parameter.Name} for method {module.GetType()}::{methodInfo.Name}.");

                parameters.Add(resolvedArgument);
            }
#if NETSTANDARD2_0
            methodInfo.Invoke(module, parameters.ToArray());
#else
            methodInfo.Invoke(module, System.Reflection.BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameters.ToArray(), culture: null!);
#endif
        }

        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder();

            builder.ConfigureHostConfiguration((configure) =>
            {
                configure.Sources.Clear();
                configure.AddConfiguration(StartupContext.BuilderSettings.HostBuilderContext.Configuration);
            });

            builder.UseContentRoot(GetContentRoot());

            var moduleAssemblyName = StartupContext.Module.GetType().Assembly.GetName().Name;
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                ctx.HostingEnvironment.ApplicationName = moduleAssemblyName;
            });

            foreach (var configureAction in StartupContext.BuilderSettings.ConfigurationActions)
            {
                builder.ConfigureAppConfiguration(configureAction);
            }

            return builder;
        }

        protected virtual string GetContentRoot()
        {
            return Path.GetFullPath(
                Path.Combine(
                    StartupContext.BuilderSettings.HostBuilderContext.HostingEnvironment
                        .ContentRootPath, "..", StartupContext.Module.Name));

        }

        public virtual IHost CreateHost(Action<IHostBuilder> configureHostBuilderAction)
        {
            var builder = CreateHostBuilder();

            foreach (var configureServicesAction in StartupContext.BuilderSettings.ConfigureServicesActions)
            {
                builder.ConfigureServices(configureServicesAction);
            }

            builder.ConfigureServices(ConfigureServices);
            configureHostBuilderAction?.Invoke(builder);
            var host = builder.Build();

            return host;
        }
    }

}