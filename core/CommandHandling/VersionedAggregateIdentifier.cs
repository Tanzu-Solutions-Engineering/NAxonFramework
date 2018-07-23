namespace NAxonFramework.CommandHandling
{
    public class VersionedAggregateIdentifier
    {
        protected bool Equals(VersionedAggregateIdentifier other)
        {
            return string.Equals(Identifier, other.Identifier) && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VersionedAggregateIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Identifier != null ? Identifier.GetHashCode() : 0) * 397) ^ Version.GetHashCode();
            }
        }

        public string Identifier { get; }
        public long? Version { get; }

        public VersionedAggregateIdentifier(string identifier, long? version)
        {
            Identifier = identifier;
            Version = version;
        }
    }
}