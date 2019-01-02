using System.Reflection;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IChildEntityDefinition
    {
        Optional<IChildEntity> CreateChildDefinition(PropertyInfo field, IEntityModel declaringEntity);
    }
}