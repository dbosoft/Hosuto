using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModulesHostBuilderContext<TModule> : IModulesHostBuilderContext<TModule> where TModule : IModule
    {
        private readonly IModuleBootstrapContext<TModule> _bootstrapContext;

        internal ModulesHostBuilderContext(HostBuilderContext hostBuilderContext, IModuleBootstrapContext<TModule> bootstrapContext)
        {
            _bootstrapContext = bootstrapContext;
            HostBuilderContext = hostBuilderContext 
                                 ?? bootstrapContext.Advanced.FrameworkServices.GetRequiredService<HostBuilderContext>();
            Advanced = new AdvancedModuleContext(_bootstrapContext.Advanced.FrameworkServices,
                bootstrapContext.Advanced.HostServices,
                _bootstrapContext);
        }

        public TModule Module => _bootstrapContext.Module;
        public IServiceProvider ModulesHostServices => _bootstrapContext.ModulesHostServices;

        public HostBuilderContext HostBuilderContext { get;  }
        public IAdvancedModuleContext Advanced { get; }


        IModule IModulesHostBuilderContext.Module => Module;
    }
}