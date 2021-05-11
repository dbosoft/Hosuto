using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class WebModuleBootstrapHostFilter<TModule> : IBootstrapHostFilter<TModule> where TModule : class
    {
        public Action<BootstrapModuleHostCommand<TModule>> Invoke(Action<BootstrapModuleHostCommand<TModule>> next)
        {
            return (command) =>
            {
                var handler = new WebModuleBootstrapHostHandler<TModule>();
                handler.BootstrapHost(command);
                next(command);
            };
        }
    }
}