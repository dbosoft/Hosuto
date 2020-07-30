using System;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class DelegateWebHostBuilderFactory : IWebModuleWebHostBuilderFactory
    {

        private readonly Func<IWebHostBuilder> _creatorFunc;

        public DelegateWebHostBuilderFactory(Func<IWebHostBuilder> creatorFunc)
        {
            _creatorFunc = creatorFunc;
        }

        public IWebHostBuilder CreateWebHost(WebModule module) => _creatorFunc();

    }
}
