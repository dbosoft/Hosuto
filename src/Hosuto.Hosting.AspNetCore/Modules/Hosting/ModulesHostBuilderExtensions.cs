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


        public static IModulesHostBuilder UseAspNetCoreWithDefaults(this IModulesHostBuilder builder, Action<IWebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services
                    .TryAddTransient<IWebModuleWebHostBuilderInitializer, WebModuleWebHostBuilderInitializerWithDefaults
                    >();
            });

            return UseAspNetCore(builder, configure);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Opt-in: host web modules on a minimal-API <see cref="Microsoft.AspNetCore.Builder.WebApplication"/>
        /// inner host instead of the classic <c>ConfigureWebHostDefaults</c> host. Additive - the
        /// module authoring model (ConfigureServices/Configure/ConfigureContainer, convention or the
        /// module interfaces) is unchanged.
        /// </summary>
        public static IModulesHostBuilder UseAspNetCoreMinimal(this IModulesHostBuilder builder,
            Action<Microsoft.AspNetCore.Builder.WebApplicationBuilder> configure = null)
        {
            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IBootstrapHostFilter<>), typeof(WebApplicationBootstrapHostFilter<>));

                if (configure != null)
                    services.AddTransient<IWebApplicationBuilderConfigurer>(sp =>
                        new DelegateWebApplicationBuilderConfigurer(configure));
            });

            return builder;
        }
#endif

        public static IModulesHostBuilder UseAspNetCore(this IModulesHostBuilder builder,
            Action<IWebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
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
        public static IModulesHostBuilder UseAspNetCore(this IModulesHostBuilder builder, Func<Microsoft.AspNetCore.Hosting.IWebHostBuilder> webHostBuilder, Action<IWebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure = null)
        {
            if (webHostBuilder == null) throw new ArgumentNullException(nameof(webHostBuilder));

            builder.ConfigureFrameworkServices((ctx, services) =>
            {
                services.AddTransient(typeof(IBootstrapHostFilter<>), typeof(WebModuleBootstrapHostFilter<>));
                services.TryAddTransient<IWebModuleWebHostBuilderFactory>(sp => new DelegateWebHostBuilderFactory(webHostBuilder));
                
            });

            return ConfigureAspNetCore(builder,configure);
        }

        public static IModulesHostBuilder ConfigureAspNetCore(this IModulesHostBuilder builder, Action<IWebModule, Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure)
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
