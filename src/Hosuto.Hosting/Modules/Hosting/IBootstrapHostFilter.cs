namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IBootstrapHostFilter<TModule> : IFilter<BootstrapModuleHostCommand<TModule>>
        where TModule : class
    {

    }

}