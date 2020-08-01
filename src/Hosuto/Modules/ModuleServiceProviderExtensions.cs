using System;
using System.Collections.Generic;
using System.Text;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules
{
    public static class ModuleServiceProviderExtensions
    {
        public static IServiceProvider AsModuleServices(this IServiceProvider serviceProvider)
        {
            var contextAccessor = serviceProvider.GetService<IModuleContextAccessor>();
            return contextAccessor?.Context != null ? contextAccessor.Context.Services : serviceProvider;
        }

    }
}
