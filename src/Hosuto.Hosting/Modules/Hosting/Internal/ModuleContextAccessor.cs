using System;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    internal class ModuleContextAccessor : IModuleContextAccessor
    {

        public IModuleContext Context { get; internal set; }
        
    }
}