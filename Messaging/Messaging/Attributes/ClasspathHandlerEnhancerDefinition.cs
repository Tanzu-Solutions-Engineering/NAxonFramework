using System;
using System.Linq;

namespace NAxonFramework.Messaging.Attributes
{
    public static class ClasspathHandlerEnhancerDefinition
    {
        public static IHandlerEnhancerDefinition Factory { get; }

        static ClasspathHandlerEnhancerDefinition()
        {
            var factoryInterface = typeof(IHandlerEnhancerDefinition);
            var factoriesInDomain = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                    assembly.ExportedTypes
                        .Where(x => factoryInterface.IsAssignableFrom(x))
                        .Select(Activator.CreateInstance)
                        .Cast<IHandlerEnhancerDefinition>()
                )
                .ToArray();
            Factory = new MultiHandlerEnhancerDefinition(factoriesInDomain);
        }
    }
}