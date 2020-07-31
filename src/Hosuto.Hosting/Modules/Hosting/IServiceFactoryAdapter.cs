﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal interface IServiceFactoryAdapter
    {
        object CreateBuilder(IServiceCollection services);

        IServiceProvider CreateServiceProvider(object containerBuilder);
    }
}