namespace NAxonFramework.CommandHandling.Model
{
    public static class IAggregateExtensions
    {
        public static string IdentifierAsString(this IAggregate aggregate)
        {
            return aggregate.Identifier?.ToString();
        }
    }
}