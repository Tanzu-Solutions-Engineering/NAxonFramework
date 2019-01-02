using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling
{
    public class MetaDataCommandTargetResolver : ICommandTargetResolver
    {
        private readonly string _identifierKey;
        private readonly string _versionKey;

        public MetaDataCommandTargetResolver(string identifierKey)
        {
            _identifierKey = identifierKey;
        }

        public MetaDataCommandTargetResolver(string identifierKey, string versionKey)
        {
            _identifierKey = identifierKey;
            _versionKey = versionKey;
        }

        public VersionedAggregateIdentifier ResolveTarget(ICommandMessage command)
        {
            var identifier = (string) command.MetaData[_identifierKey];
            Assert.NotNull(identifier, () => "The MetaData for the command does not exist or contains a null value");
            var version = (long?) (_versionKey == null ? null : command.MetaData[_versionKey]);
            return new VersionedAggregateIdentifier(identifier, version);
        }
    }
}