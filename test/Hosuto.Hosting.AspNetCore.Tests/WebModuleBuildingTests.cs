using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hosuto.Hosting.AspNetCore.Tests
{
    public class WebModuleBuildingTests
    {


        [Fact]
        public void Build_module_has_configuration_from_host()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeWebModule>();
            builder.ConfigureAppConfiguration(c => 
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["test"] = "testValue"
                }));

            var host = builder.Build();

            var module = host.Services.GetRequiredService<SomeWebModule>();
            Assert.Equal("testValue", module.Configuration["test"]);
        }

        [Fact]
        public void AppConfiguration_is_applied_to_all_Configurations()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeWebModule>();
#if !NETCOREAPP2_1
            builder.UseAspNetCore();
#endif
            builder.ConfigureHostConfiguration(c =>
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["setting"] = "hostValue"
                }));
            builder.ConfigureAppConfiguration(c =>
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["setting"] = "appValue"
                }));

            var host = builder.Build();
            var hostConfig = host.Services.GetRequiredService<IConfiguration>();
            var module = host.Services.GetRequiredService<SomeWebModule>();
            Assert.Equal("appValue", hostConfig["setting"]);
            Assert.Equal("appValue", module.HostConfiguration["setting"]);
            Assert.Equal("appValue", module.Configuration["setting"]);

        }

        [Theory]
        [InlineData("firstValue", null, null, "firstValue", "firstValue")]
        [InlineData("wrongValue","appValue", null, "appValue", "appValue")]
        [InlineData("wrongValue", "appValue", "moduleValue", "appValue", "moduleValue")]
        public void Configuration_is_applied_in_order(string firstValue, 
            string secondValue, string moduleValue, string hostExpected, string moduleExpected)
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.ConfigureAppConfiguration(c =>
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["setting"] = firstValue
                }));

            builder.HostModule<SomeWebModule>(options =>
            {
                if (moduleValue!=null)
                {
                    options.Configure(mb =>
                    {
                        mb.ConfigureAppConfiguration(c =>
                            c.AddInMemoryCollection(new Dictionary<string, string>
                            {
                                ["setting"] = moduleValue
                            }));
                    });
                }
            });
#if !NETCOREAPP2_1
            builder.ConfigureWebHost(cfg => { });
#endif
            builder.ConfigureHostConfiguration(c =>
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["setting"] = "should never be seen"
                }));

            if (secondValue != null)
            {
                builder.ConfigureAppConfiguration(c =>
                    c.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["setting"] = secondValue
                    }));
            }


            var host = builder.Build();
            var module = host.Services.GetRequiredService<SomeWebModule>();


            Assert.Equal(hostExpected, module.HostConfiguration["setting"]);
            Assert.Equal(moduleExpected, module.Configuration["setting"]);

        }

        [Fact]
        public void Build_module_has_logger_from_host()
        {
            var builder = ModulesHost.CreateDefaultBuilder();

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.Log(LogLevel.Trace,
                        It.IsAny<EventId>(), null, It.IsAny<Exception>(), 
                        It.IsAny<Func<object, Exception, string>>())).Verifiable();

                var logProviderMoq = new Mock<ILoggerProvider>();
            logProviderMoq.Setup(x => 
                x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            builder.HostModule<SomeWebModule>();

            builder.ConfigureLogging(lb =>
                lb.SetMinimumLevel(LogLevel.Trace)
                .AddProvider(logProviderMoq.Object));

            var host = builder.Build();

            var moduleHost = host.Services.GetRequiredService<IModuleHost<SomeWebModule>>();
            var loggerFactory = moduleHost.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("dummy");
            logger.Log<object>(LogLevel.Trace, new EventId(0), null, null, (s, e) => "trace message");

            loggerMock.Verify();
        }

        [Fact]
        public void Multiple_modules_can_be_bootstrapped_and_see_each_other()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeWebModule>();
            builder.HostModule<OtherModule>();

            var host = builder.Build();
            var module = host.Services.GetRequiredService<OtherModule>();

            Assert.NotNull(module.SomeModule);
        }


        private class OtherModule : IModule
        {
            public string Name => "I'm a module, too";
            public SomeWebModule SomeModule { get; private set;  }

            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                SomeModule = sp.GetRequiredService<SomeWebModule>();
            }

        }
    }
}