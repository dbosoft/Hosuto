
using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules;
using Microsoft.AspNetCore.Hosting;

namespace Hosuto.AspNetCore
{
    internal class DelegateWebHostBuilderFactory : IWebModuleWebHostBuilderFactory
    {

        private readonly Func<IWebHostBuilder> _creatorFunc;

        public DelegateWebHostBuilderFactory(Func<IWebHostBuilder> creatorFunc)
        {
            _creatorFunc = creatorFunc;
        }

        public IWebHostBuilder CreateWebHost(IWebModule module) => _creatorFunc();

    }
}
