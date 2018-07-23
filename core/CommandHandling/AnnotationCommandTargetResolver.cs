using System;
using System.Reflection;

namespace NAxonFramework.CommandHandling
{
    public class AnnotationCommandTargetResolver : ICommandTargetResolver
    {
        public VersionedAggregateIdentifier ResolveTarget(ICommandMessage command)
        {
            string aggregateIdentifier;
            long? aggregateVersion;
            try
            {
                aggregateIdentifier = FindIdentifier(command);
                aggregateVersion = FindVersion(command);
            }
            catch (FormatException e)
            {
                throw new ArgumentException("The value provided for the version is not a number.", e);
            }
            catch (Exception e)
            {
                if(e is TargetInvocationException || e is TargetException)
                    throw new ArgumentException("An exception occurred while extracting aggregate information form a command", e);
                throw;
            }

            if (aggregateIdentifier == null)
            {
                throw new ArgumentException("Invalid command. It does not identify the target aggregate. "
                                            + $"Make sure at least one of the fields or methods in the {command.PayloadType.Name} class contains the "
                                            + "[TargetAggregateIdentifier] attribute and that it returns a non-null value.");
            }
            return new VersionedAggregateIdentifier(aggregateIdentifier, aggregateVersion);
        }

        private string FindIdentifier(ICommandMessage command)
        {
            foreach (var property in command.PayloadType.GetProperties())
            {
                if(property.IsDefined(typeof(TargetAggregateIdentifierAttribute), true))
                    return property.GetValue(command)?.ToString();
            }

            return null;
        }
        
        private long? FindVersion(ICommandMessage command)
        {
            foreach (var property in command.PayloadType.GetProperties())
            {
                if (property.IsDefined(typeof(TargetAggregateVersionAttribute), true))
                {
                    var val = property.GetValue(command);
                    if (val == null) return null;
                    return Convert.ToInt64(val);
                }
            }
            return null;
        }
    }
}