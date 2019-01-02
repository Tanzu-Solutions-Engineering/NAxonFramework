using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NAxonFramework.Common
{
    public abstract class IdentifierFactory
    {
        private static IdentifierFactory _instance;

        public abstract string GenerateIdentifier();

        public static IServiceProvider ServiceProvider { get; set; }
        public static ILogger Logger { get; set; }
        public static IdentifierFactory Instance  
        {
            get
            {
                if (_instance == null)
                {
                    var factories = (List<IdentifierFactory>)ServiceProvider?.GetService(typeof(List<IdentifierFactory>)) ?? ScanForFactories();
                    if(factories.Count > 1)
                        Logger.LogWarning("More than one IdentifierFactory implementation was found. This may result in different selections being made after restart of the application");
                    var factory = factories.FirstOrDefault();
                    if (factory == null)
                    {
                        factory = new DefaultIdentifierFactory();
                        Logger?.LogDebug("Using default GUID-based IdentifierFactory");
                    }
                    else
                    {
                        Logger?.LogInformation($"Found custom IdentifierFactory implementation: {factory.GetType().Name}");
                    }

                    _instance = factory;
                }

                return _instance;
            }
        }

        private static List<IdentifierFactory> ScanForFactories()
        {
            Logger?.LogDebug("Looking for IdentifierFactory implementation using the load assemblies");
            var factories = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.DefinedTypes)
                .Where(x => typeof(IdentifierFactory).IsAssignableFrom(x) && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IdentifierFactory>()
                .ToList();
            if (factories.Any())
            {
                Logger?.LogDebug($"Found IdentifierFactory implementation using the {factories.First()}");
            }

            return factories;
        }
        
        
    }
}