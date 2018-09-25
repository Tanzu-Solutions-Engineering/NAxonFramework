namespace NAxonFramework.Common.Property
{
    public interface IProperty<in T>
    {
        V GetValue<V>(T target);
    }
}