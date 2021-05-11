using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IAddSimpleInjectorFilter : IFilter<IModulesHostBuilderContext, SimpleInjectorAddOptions>
    {

    }

    public interface IAddSimpleInjectorFilter<TModule> : IFilter<IModulesHostBuilderContext<TModule>, SimpleInjectorAddOptions> where TModule : class
    {

    }
}