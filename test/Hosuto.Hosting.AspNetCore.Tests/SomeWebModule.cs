using Dbosoft.Hosuto.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hosuto.Hosting.AspNetCore.Tests
{
    public interface ISomeService
    {
        void CallMe();
    }

    public class SomeServiceImplementation : ISomeService
    {
        public void CallMe()
        {
        }
    }

    public class SomeWebModule : WebModule
    {
        public IConfiguration HostConfiguration { get; }
        public IConfiguration Configuration { get; private set; }
        
        public override string Path => "path";

#if NETCOREAPP2_1
        public IHostingEnvironment Environment { get; private set; }
#else
        public IWebHostEnvironment Environment { get; private set; }
#endif

        public SomeWebModule(IConfiguration hostConfiguration)
        {
            HostConfiguration = hostConfiguration;
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Configuration = configuration;
            services.AddTransient<ISomeService, SomeServiceImplementation>();

            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app,
#if NETCOREAPP2_1
        IHostingEnvironment env
#else
        IWebHostEnvironment env
#endif

        )
        {

#if NETCOREAPP2_1
            app.UseRouter(route =>
#else
            app.UseRouting();
            app.UseEndpoints(route =>
#endif
            {
                route.MapGet("/", context => HttpResponseWritingExtensions.WriteAsync(context.Response, "Ok"));
            });


            app.ApplicationServices.GetRequiredService<ISomeService>().CallMe();
        }

    }


}