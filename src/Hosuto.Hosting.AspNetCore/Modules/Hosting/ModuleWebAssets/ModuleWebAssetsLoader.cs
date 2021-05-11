// based on https://github.com/dotnet/aspnetcore/blob/main/src/Hosting/Hosting/src/StaticWebAssets/StaticWebAssetsLoader.cs
// licensed under Apache License, Version 2.0 by .NET Foundation

#if !NETSTANDARD

#nullable enable

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Dbosoft.Hosuto.Modules.Hosting.ModuleWebAssets
{
    /// <summary>
    /// Loader for static web assets for modules
    /// </summary>
    public class ModuleWebAssetsLoader
    {
        internal const string StaticWebAssetsManifestName = "Microsoft.AspNetCore.StaticWebAssets.xml";

        /// <summary>
        /// Configure the <see cref="IWebHostEnvironment"/> to use static web assets.
        /// </summary>
        /// <param name="environment">The application <see cref="IWebHostEnvironment"/>.</param>
        /// <param name="configuration">The host <see cref="IConfiguration"/>.</param>
        /// <param name="hostingApplicationName">name of application actually hosting the module</param>
        public static void UseStaticWebAssets(IWebHostEnvironment environment, IConfiguration configuration, 
            string hostingApplicationName)
        {
            using var manifest = ResolveManifest(hostingApplicationName, configuration);
            if (manifest != null)
            {
                UseStaticWebAssetsCore(environment, manifest);
            }
        }

        /// <summary>
        /// Configure the <see cref="IWebHostEnvironment"/> to use published web assets.
        /// </summary>
        /// <param name="environment">The application <see cref="IWebHostEnvironment"/>.</param>
        /// <param name="configuration">The host <see cref="IConfiguration"/>.</param>
        // ReSharper disable once InconsistentNaming
        public static void UseModuleAssets(IWebHostEnvironment environment, IConfiguration configuration)
        {
            var rootPath = configuration.GetValue<string>("moduleAssetsRoot");

            if (rootPath == null)
            {
                if (environment.ContentRootPath == null)
                    return;

                rootPath = Path.Combine(environment.ContentRootPath, "wwwroot", ".modules");
            }

            var moduleDir = new DirectoryInfo(Path.Combine(rootPath, environment.ApplicationName));
            
            if (!moduleDir.Exists)
                return;

            var webRootFileProvider = environment.WebRootFileProvider;
            var additionalFiles = new[]
            {
                //first try module, then app
                new PhysicalFileProvider(moduleDir.FullName), webRootFileProvider
            };
            environment.WebRootFileProvider = new CompositeFileProvider(additionalFiles);


        }

        internal static void UseStaticWebAssetsCore(IWebHostEnvironment environment, Stream manifest)
        {
            var webRootFileProvider = environment.WebRootFileProvider;
            var moduleName = environment.ApplicationName;

            var additionalFiles = StaticWebAssetsReader.Parse(manifest)
                .Where(m => IsModuleOrOtherContent(moduleName, m))
                .Select(cr => new StaticWebAssetsFileProvider(
                    StripModulePath(cr.BasePath), cr.Path))
                .OfType<IFileProvider>() // Upcast so we can insert on the resulting list.
                .ToList();

            if (additionalFiles.Count == 0)
            {
                return;
            }
            else
            {
                additionalFiles.Insert(0, webRootFileProvider);
                environment.WebRootFileProvider = new CompositeFileProvider(additionalFiles);
            }
        }

        private static string StripModulePath(string path)
        {
            var strippedPath = path.TrimStart('/', '\\');

            return !strippedPath.StartsWith(".modules") ? path : "";
        }

        private static bool IsModuleOrOtherContent(string moduleName, StaticWebAssetsReader.ContentRootMapping mapping)
        {
            var path = mapping.BasePath.TrimStart('/','\\');

            if (!path.StartsWith(".modules"))
                return true;

            path = path.Remove(0,".modules".Length).TrimStart('/', '\\');
            return path == moduleName;
        }


        internal static Stream? ResolveManifest(string hostingApplicationName, IConfiguration configuration)
        {
            try
            {
                var manifestPath = configuration.GetValue<string>(WebHostDefaults.StaticWebAssetsKey);
                var filePath = manifestPath ?? ResolveRelativeToAssembly(hostingApplicationName);

                if (filePath != null && File.Exists(filePath))
                {
                    return File.OpenRead(filePath);
                }
                else
                {
                    // A missing manifest might simply mean that the feature is not enabled, so we simply
                    // return early. Misconfigurations will be uncommon given that the entire process is automated
                    // at build time.
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string? ResolveRelativeToAssembly(string hostingApplicationName)
        {
            var assembly = Assembly.Load(hostingApplicationName);
            if (string.IsNullOrEmpty(assembly.Location))
            {
                return null;
            }

            return Path.Combine(Path.GetDirectoryName(assembly.Location)!, $"{hostingApplicationName}.StaticWebAssets.xml");
        }
    }
}
#nullable restore

#endif