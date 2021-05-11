using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IConfigureContainerFilter : IFilter<IModuleContext, Container>
    {
    }

    public interface IConfigureContainerFilter<TModule> : IFilter<IModuleContext<TModule>, Container> where TModule : class
    {
    }
}