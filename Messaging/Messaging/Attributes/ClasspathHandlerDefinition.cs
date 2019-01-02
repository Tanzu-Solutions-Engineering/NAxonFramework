using System;
using System.Linq;

namespace NAxonFramework.Messaging.Attributes
{
    public static class ClasspathHandlerDefinition
    {
        public static MultiHandlerDefinition Factory { get; }

        static ClasspathHandlerDefinition()
        {
            var factoryInterface = typeof(IHandlerDefinition);
            var factoriesInDomain = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                    assembly.ExportedTypes
                        .Where(x => factoryInterface.IsAssignableFrom(x))
                        .Select(Activator.CreateInstance)
                        .Cast<IHandlerDefinition>()
                )
                .ToList();
            Factory = new MultiHandlerDefinition(factoriesInDomain);
        }
    }
}