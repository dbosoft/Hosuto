using System;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{

    public class BootstrapModuleHostCommand<TModule> where TModule : class
    {
        public IModuleBootstrapContext<TModule> BootstrapContext { get; set; }
        public ModuleHostingOptions Options { get; set; }
        public IHost Host { get; set; }
        public IModuleContext<TModule> ModuleContext { get; set; }
    }
}