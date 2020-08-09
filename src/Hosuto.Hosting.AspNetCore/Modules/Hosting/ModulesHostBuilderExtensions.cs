using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModulesHostBuilderExtensions
    {
#if NETCOREAPP

        public static IModulesHostBuilder ConfigureWebHostDefaults(this IModulesHostBuilder builder, Action<Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            return builder.UseAspNetCoreWithDefaults((module, webHostBuilder) =>
            {
                configure(webHostBuilder);
            });
        }

        public static IModulesHostBuilder ConfigureWebHost(this IModulesHostBuilder builder, Action<Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            return builder.UseAspNetCore((module, webHostBuilder) =>
            {
                configure(webHostBuilder);
            });
        }


        public static IModulesHostBuilder UseAspNetCoreWithDefaults(this IModulesHostBuilder builder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services
                    .TryAddTransient<IWebModuleWebHostBuilderInitializer, WebModuleWebHostBuilderInitializerWithDefaults
                    >();
            });

            return UseAspNetCore(builder, configure);
        }

        public static IModulesHostBuilder UseAspNetCore(this IModulesHostBuilder builder,
            Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IBootstrapHostFilter<>), typeof(WebModuleBootstrapHostFilter<>));
                services.TryAddTransient<IWebModuleWebHostBuilderInitializer, WebModuleWebHostBuilderInitializer>();

                if(configure!=null)
                    services.AddTransient<IWebModuleWebHostBuilderFilter>(sp=>new DelegateWebModuleWebHostBuilderFilter(configure));

            });

            return builder;
        }

#else
        public static IModulesHostBuilder UseAspNetCore(this IModulesHostBuilder builder, Func<Microsoft.AspNetCore.Hosting.IWebHostBuilder> webHostBuilder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            if (webHostBuilder == null) throw new ArgumentNullException(nameof(webHostBuilder));

            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IBootstrapHostFilter<>), typeof(WebModuleBootstrapHostFilter<>));
                services.TryAddTransient<IWebModuleWebHostBuilderFactory>(sp => new DelegateWebHostBuilderFactory(webHostBuilder));
                
            });

            return ConfigureAspNetCore(builder,configure);
        }

        public static IModulesHostBuilder ConfigureAspNetCore(this IModulesHostBuilder builder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.TryAddTransient<IWebModuleWebHostBuilderFilter>(sp => new DelegateWebModuleWebHostBuilderFilter(configure));
            });

            return builder;
        }
#endif
    }
}
