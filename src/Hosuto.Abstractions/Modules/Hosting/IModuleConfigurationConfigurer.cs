﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleConfigurationConfigurer
    {
        void ConfigureModuleConfiguration(IModulesHostBuilderContext context, IConfigurationBuilder configuration);
    }


}