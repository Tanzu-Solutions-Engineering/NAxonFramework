namespace NAxonFramework.CommandHandling.Model
{
    public static class IAggregateExtensions
    {
        public static string IdentifierAsString<T>(this IAggregate<T> aggregate)
        {
            return aggregate.Identifier?.ToString();
        }
    }
}