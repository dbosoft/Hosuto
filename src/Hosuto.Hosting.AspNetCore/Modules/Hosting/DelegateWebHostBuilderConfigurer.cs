using System;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class DelegateWebHostBuilderConfigurer : IWebModuleWebHostBuilderConfigurer
    {
        private readonly Action<WebModule, IWebHostBuilder> _configureDelegate;

        public DelegateWebHostBuilderConfigurer(Action<WebModule, IWebHostBuilder> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public void ConfigureWebHost(WebModule module, IWebHostBuilder builder) => _configureDelegate(module, builder);

    }
}