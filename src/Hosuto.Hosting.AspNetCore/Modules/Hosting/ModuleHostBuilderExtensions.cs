using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
#if NETCOREAPP

        public static IModuleHostBuilder ConfigureWebHostDefaults(this IModuleHostBuilder builder, Action<Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            return builder.UseAspNetCoreWithDefaults((module, webHostBuilder) =>
            {
                configure(webHostBuilder);
            });
        }

        public static IModuleHostBuilder ConfigureWebHost(this IModuleHostBuilder builder, Action<Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            return builder.UseAspNetCore((module, webHostBuilder) =>
            {
                configure(webHostBuilder);
            });
        }


        public static IModuleHostBuilder UseAspNetCoreWithDefaults(this IModuleHostBuilder builder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services
                    .TryAddTransient<IWebModuleWebHostBuilderInitializer, WebModuleWebHostBuilderInitializerWithDefaults
                    >();
            });

            return UseAspNetCore(builder, configure);
        }

        public static IModuleHostBuilder UseAspNetCore(this IModuleHostBuilder builder,
            Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleStartupHandlerFactory, WebModuleStartupFactory>();
                services.TryAddTransient<IWebModuleWebHostBuilderInitializer, WebModuleWebHostBuilderInitializer>();

                if(configure!=null)
                    services.AddTransient<IWebModuleWebHostBuilderConfigurer>(sp=>new DelegateWebHostBuilderConfigurer(configure));

            });

            return builder;
        }

#else
        public static IModuleHostBuilder UseAspNetCore(this IModuleHostBuilder builder, Func<Microsoft.AspNetCore.Hosting.IWebHostBuilder> webHostBuilder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            if (webHostBuilder == null) throw new ArgumentNullException(nameof(webHostBuilder));

            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient<IModuleStartupHandlerFactory, WebModuleStartupFactory>();
                services.TryAddTransient<IWebModuleWebHostBuilderFactory>(sp => new DelegateWebHostBuilderFactory(webHostBuilder));
                
            });

            return ConfigureAspNetCore(builder,configure);
        }

        public static IModuleHostBuilder ConfigureAspNetCore(this IModuleHostBuilder builder, Action<WebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.TryAddTransient<IWebModuleWebHostBuilderConfigurer>(sp => new DelegateWebHostBuilderConfigurer(configure));
            });

            return builder;
        }
#endif
    }
}
