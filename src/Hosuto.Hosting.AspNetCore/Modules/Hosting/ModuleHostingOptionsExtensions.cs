using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostingOptionsExtensions
    {
#if NETSTANDARD

        public static IModuleHostingOptions Configure(this IModuleHostingOptions options, Action<IWebHostBuilder> configure)
        {
            if (!options.Properties.ContainsKey("ConfigureWebHostAction"))
                options.Properties.Add("ConfigureWebHostAction", configure);
            return options;
        }

        internal static IModuleHostingOptions ConfigureWebHostBuilder(this IModuleHostingOptions options, IWebHostBuilder builder)
        {
            if (!options.Properties.ContainsKey("ConfigureWebHostAction"))
                return options;
            
            var configureDelegate = (Action<IWebHostBuilder>)options.Properties["ConfigureWebHostAction"]; 
            configureDelegate?.Invoke(builder);

            return options;
        }

        public static IModuleHostingOptions BuildWebHostCallback(this IModuleHostingOptions options, Func<IWebHostBuilder, IWebHost> buildDelegate)
        {
            if(!options.Properties.ContainsKey("BuildWebHostAction"))
                options.Properties.Add("BuildWebHostAction", buildDelegate);
            return options;
        }

        internal static IWebHost BuildWebHost(this IModuleHostingOptions options, IWebHostBuilder builder)
        {
            if(!options.Properties.ContainsKey("BuildWebHostAction"))
                return builder.Build();

            var buildDelegate = (Func<IWebHostBuilder, IWebHost>) options.Properties["BuildWebHostAction"];
            return buildDelegate(builder);
        }
#endif
    }
}
