﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public class ModuleStartupContext<TModule> : IDisposable where TModule : IModule
    {

        public ModuleStartupContext(TModule module, ModuleHostBuilderSettings builderSettings, IServiceProvider serviceProvider)
        {
            BuilderSettings = builderSettings;
            ServiceProvider = serviceProvider;
            Module = module;
        }

        public TModule Module { get; }
        public IServiceProvider ServiceProvider { get; }
        public ModuleHostBuilderSettings BuilderSettings { get; }


        protected virtual void Dispose(bool disposing)
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}