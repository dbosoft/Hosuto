using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DefaultModuleHostFactory : IModuleHostFactory
    {


        public IModuleHost CreateModuleHost(
            Dictionary<Type, Action<IHostBuilder>> modules, 
            ModuleHostBuilderSettings builderSettings,
            IServiceProvider serviceProvider)
        {
            switch (modules.Count)
            {
                case 0:
                    throw new InvalidOperationException("No modules registered in host builder.");
                case 1:
                {
                    var moduleEntry = modules.First();
                    var module = (IModule)serviceProvider.GetRequiredService(moduleEntry.Key);
                    return CreateModuleHost(module,
                        builderSettings,
                        serviceProvider, 
                        moduleEntry.Value);
                }
                default:
                {
                    return new ModuleCollectionHost(
                        modules.Select(moduleEntry
                            => CreateModuleHost(
                                (IModule)serviceProvider.GetRequiredService(moduleEntry.Key),
                                builderSettings,
                                serviceProvider,
                                moduleEntry.Value)));
                }
            }
        }

        protected virtual IModuleHost CreateModuleHost(IModule module, 
            ModuleHostBuilderSettings hostBuilderSettings,
            IServiceProvider serviceProvider,
            Action<IHostBuilder> configureHostBuilderAction)
        {

            var hostType = typeof(ModuleHost<>).MakeGenericType(module.GetType());
            return (IModuleHost) Activator.CreateInstance(hostType, module,hostBuilderSettings, configureHostBuilderAction, serviceProvider);

        }
    }
}