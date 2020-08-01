using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class ModuleHostBootstrapActions
    {
        public Action<IServiceProvider> Bootstrap { get; set; }
        public Action<IHostBuilder> ConfigureBuilder { get; set; }

    }
}