using Dbosoft.Hosuto.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        public override string Name => "I'm a module";
        public override string Path => "path";


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISomeService, SomeServiceImplementation>();

            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
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