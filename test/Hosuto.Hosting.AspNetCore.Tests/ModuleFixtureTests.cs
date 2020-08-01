using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hosuto.Hosting.AspNetCore.Tests
{
    public class ModuleFixtureTests : IClassFixture<WebModuleFactory<SomeWebModule>>
    {
        private readonly WebModuleFactory<SomeWebModule> _factory;

        public ModuleFixtureTests(WebModuleFactory<SomeWebModule> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SomeModuleRepliesWithOk()
        {
            var result = await _factory.WithWebHostBuilder(b =>
                {
                    b.UseSolutionRelativeContentRoot("");
                })
                .CreateClient().GetStringAsync("/");

            Assert.Equal("Ok", result);
        }
    }

    public class SomeWebModule : WebModule
    {
        public override string Name => "I'm a module";
        public override string Path => "path";


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(route =>
            {
                route.MapGet("/", context => context.Response.WriteAsync("Ok"));
            });
        }

    }


}