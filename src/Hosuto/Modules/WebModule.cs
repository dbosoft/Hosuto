using Dbosoft.Hosuto.Modules;

namespace Hosuto.AspNetCore.Hosting
{
    public abstract class WebModuleBase : ModuleBase
    {
        public abstract string Path { get; }

    }
}