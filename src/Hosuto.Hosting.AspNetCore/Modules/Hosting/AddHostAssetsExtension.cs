#if !NETSTANDARD

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using Microsoft.AspNetCore.Hosting.StaticWebAssets;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class AddHostAssetsExtension
    {
        public static IModulesHostBuilder AddHostAssets<TModule>(this IModulesHostBuilder builder, [AllowNull] string assetsName = null)
            where TModule : WebModule
        {
            builder.ConfigureFrameworkServices((_, sc) =>
            {
                sc.AddTransient<IWebModuleWebHostBuilderFilter>(
                    sp => new UseHostAssetsFilter<TModule>(
                        sp.GetRequiredService<IHostEnvironment>(), assetsName)
                );
            });

            return builder;
        }


        private class UseHostAssetsFilter<TModule> : IWebModuleWebHostBuilderFilter where TModule : WebModule
        {
            private readonly string _assetName;

            public UseHostAssetsFilter(IHostEnvironment environment, string assetName)
            {
                _assetName = assetName ?? environment.ApplicationName;
            }

            public Action<WebModule, IWebHostBuilder> Invoke(Action<WebModule, IWebHostBuilder> next)
            {
                return (webModule, builder) =>
                {
                    if (webModule is TModule)
                    {

                        builder.ConfigureAppConfiguration((ctx, config) =>
                        {
                            var oldName = ctx.HostingEnvironment.ApplicationName;
                            ctx.HostingEnvironment.ApplicationName = _assetName;
                            if (ctx.HostingEnvironment.IsDevelopment())
                            {
                                StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration);
                            }

                            ctx.HostingEnvironment.ApplicationName = oldName;
                        });
                    }

                    next(webModule, builder);

                };
            }
        }
    }
}

#endif