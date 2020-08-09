using System;
using System.Collections.Generic;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public static class ModuleMethodInvoker
    {
        public static void CallOptionalMethod(IModuleContext moduleContext, string methodName, params object[] arguments)
        {
            var moduleType = moduleContext.Module.GetType();
            var methodInfo = moduleType.GetMethod(methodName);

            if (methodInfo == null)
                return;

            var parameters = new List<object>();
            for (var index = 0; index < methodInfo.GetParameters().Length; index++)
            {
                var parameter = methodInfo.GetParameters()[index];

                //by convention: when the first argument is a IServiceProvider it is the service provider of ModuleHost
                if (index == 0 && parameter.ParameterType == typeof(IServiceProvider))
                {
                    parameters.Add(moduleContext.ModulesHostServices);
                    continue;
                }

                var parameterFound = false;

                foreach (var argument in arguments)
                {
                    if (!parameter.ParameterType.IsInstanceOfType(argument)) continue;

                    parameters.Add(argument);
                    parameterFound = true;
                    break;
                }

                if (parameterFound)
                    continue;

                //resolve all other services from module service provider

                if(moduleContext.Advanced.HostServices==null)
                    throw new InvalidOperationException($"Argument {parameter.Name} with type {parameter.ParameterType} is not support for method {moduleContext.Module.GetType()}::{methodInfo.Name}. For this method no additional arguments can be injected.");

                var resolvedArgument = moduleContext.Advanced.HostServices.GetService(parameter.ParameterType);
                if (resolvedArgument == null)
                    throw new InvalidOperationException(
                        $"Failed to resolve type {parameter.ParameterType} of argument {parameter.Name} for method {moduleContext.Module.GetType()}::{methodInfo.Name}.");

                parameters.Add(resolvedArgument);
            }
#if NETSTANDARD2_0
            methodInfo.Invoke(moduleContext.Module, parameters.ToArray());
#else
            methodInfo.Invoke(moduleContext.Module, System.Reflection.BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameters.ToArray(), culture: null!);
#endif
        }

    }
}