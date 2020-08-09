using System;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class DelegateWebModuleWebHostBuilderFilter : IWebModuleWebHostBuilderFilter
    {
        private readonly Action<WebModule, IWebHostBuilder> _configureDelegate;

        public DelegateWebModuleWebHostBuilderFilter(Action<WebModule, IWebHostBuilder> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public Action<WebModule, IWebHostBuilder> Invoke(Action<WebModule, IWebHostBuilder> next)
        {
            return (webModule, builder) =>
            {
                _configureDelegate(webModule, builder);
                next(webModule, builder);
            };
        }
    }
}