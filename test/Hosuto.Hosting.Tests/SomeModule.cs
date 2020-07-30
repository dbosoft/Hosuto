using System;
using System.Security.Authentication.ExtendedProtection;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hosuto.Hosting.Tests
{
    public class SomeModule : IModule
    {
        public string Name => "I'm a module";

        public SomeModule(IHostEnvironment environment)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IServiceProvider sp)
        {

        }

        public void ConfigureContainer(IServiceProvider sp)
        {

        }
    }


    public class SomeWebModule : WebModule
    {
        public void ConfigureServices(IServiceCollection services, IServiceProvider sp)
        {

        }

        public override string Path => "";
        public override string Name  => "I'm a web module";
    }
}
