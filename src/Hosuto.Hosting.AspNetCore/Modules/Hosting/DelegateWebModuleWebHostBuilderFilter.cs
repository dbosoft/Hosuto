using System;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class DelegateWebModuleWebHostBuilderFilter : IWebModuleWebHostBuilderFilter
    {
        private readonly Action<IWebModule, IWebHostBuilder> _configureDelegate;

        public DelegateWebModuleWebHostBuilderFilter(Action<IWebModule, IWebHostBuilder> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public Action<IWebModule, IWebHostBuilder> Invoke(Action<IWebModule, IWebHostBuilder> next)
        {
            return (webModule, builder) =>
            {
                _configureDelegate(webModule, builder);
                next(webModule, builder);
            };
        }
    }
}