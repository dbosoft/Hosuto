using System;
using System.Collections.Generic;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public static class ModuleMethodInvoker
    {
        public static void CallOptionalMethod(IModule module, string methodName, IServiceProvider outerServiceProvider, IServiceProvider serviceProvider, params object[] arguments)
        {
            var moduleType = module.GetType();
            var methodInfo = moduleType.GetMethod(methodName);

            if (methodInfo == null)
                return;

            var parameters = new List<object>();
            for (var index = 0; index < methodInfo.GetParameters().Length; index++)
            {
                var parameter = methodInfo.GetParameters()[index];

                //by convention: when the first argument is a IServiceProvider it is the outer container
                if (index == 0 && parameter.ParameterType == typeof(IServiceProvider))
                {
                    parameters.Add(outerServiceProvider);
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

                //resolve all other services from serviceProvider
                var resolvedArgument = serviceProvider.GetService(parameter.ParameterType);
                if (resolvedArgument == null)
                    throw new InvalidOperationException(
                        $"Failed to resolve type {parameter.ParameterType} of argument {parameter.Name} for method {module.GetType()}::{methodInfo.Name}.");

                parameters.Add(resolvedArgument);
            }
#if NETSTANDARD2_0
            methodInfo.Invoke(module, parameters.ToArray());
#else
            methodInfo.Invoke(module, System.Reflection.BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameters.ToArray(), culture: null!);
#endif
        }

    }
}