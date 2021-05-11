using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IUseSimpleInjectorFilter : IFilter<IModuleContext, SimpleInjectorUseOptions>
    {

    }

    public interface IUseSimpleInjectorFilter<TModule> : IFilter<IModuleContext<TModule>, SimpleInjectorUseOptions> where TModule : class
    {

    }
}