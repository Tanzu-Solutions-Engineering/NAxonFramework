using System;

namespace NAxonFramework.Common
{
    public class DefaultIdentifierFactory : IdentifierFactory
    {
        public override string GenerateIdentifier() => Guid.NewGuid().ToString();
    }
}