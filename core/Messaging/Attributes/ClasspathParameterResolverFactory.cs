using System;
using System.Linq;
using Microsoft.Extensions.DependencyModel;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    public static class ClasspathParameterResolverFactory
    {
        public static IParameterResolverFactory Factory { get; }

        static ClasspathParameterResolverFactory()
        {
            var factoryInterface = typeof(IParameterResolverFactory);
            var factoriesInDomain = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                    assembly.ExportedTypes
                        .Where(x => factoryInterface.IsAssignableFrom(x))
                        .Select(Activator.CreateInstance)
                        .Cast<IParameterResolverFactory>()
                )
                .ToList();
            Factory = MultiParameterResolverFactory.Ordered(factoriesInDomain);
        }
    }
}