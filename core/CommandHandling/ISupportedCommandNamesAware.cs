using System.Collections.Generic;

namespace NAxonFramework.CommandHandling
{
    public interface ISupportedCommandNamesAware
    {
        ISet<string> SupportedCommandNames { get; }
    }
}