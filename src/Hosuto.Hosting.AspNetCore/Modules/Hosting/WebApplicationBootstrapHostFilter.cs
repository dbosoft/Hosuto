#if NET6_0_OR_GREATER
using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class WebApplicationBootstrapHostFilter<TModule> : IBootstrapHostFilter<TModule> where TModule : class
    {
        public Action<BootstrapModuleHostCommand<TModule>> Invoke(Action<BootstrapModuleHostCommand<TModule>> next)
        {
            return command =>
            {
                var handler = new WebApplicationModuleHostHandler<TModule>();
                handler.BootstrapHost(command);
                next(command);
            };
        }
    }
}
#endif
