using System;
using Dbosoft.Hosuto.Modules;
using Microsoft.AspNetCore.Hosting;

namespace Hosuto.AspNetCore.Hosting
{
    class DelegateWebHostBuilderConfigurer : IWebModuleWebHostBuilderConfigurer
    {
        private readonly Action<IWebModule, IWebHostBuilder> _configureDelegate;

        public DelegateWebHostBuilderConfigurer(Action<IWebModule, IWebHostBuilder> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public void ConfigureWebHost(IWebModule module, IWebHostBuilder builder) => _configureDelegate(module, builder);

    }
}