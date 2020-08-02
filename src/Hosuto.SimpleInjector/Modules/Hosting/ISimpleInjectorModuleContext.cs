using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface ISimpleInjectorModuleContext : IModuleContext
    {
        Container Container
        {
            get; 
        }
    }
}