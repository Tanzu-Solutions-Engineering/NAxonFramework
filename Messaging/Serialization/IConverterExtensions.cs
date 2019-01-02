namespace NAxonFramework.Serialization
{
    public static class IConverterExtensions
    {
        public static ISerializedObject<T> Convert<T>(this IConverter converter, ISerializedObject original)
        {
            if (original.ContentType == typeof(T))
                return (ISerializedObject<T>) original;
            return new SimpleSerializedObject<T>(converter.Convert<T>(original.Data, original.ContentType), original.Type);
        }

        public static T Convert<T>(this IConverter converter, object source)
        {
            return (T)converter.Convert<T>(source, source.GetType());
        }
    }
}