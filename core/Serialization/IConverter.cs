using System;

namespace NAxonFramework.Serialization
{
    public interface IConverter
    {
        bool CanConvert<T>(Type sourceType);
        
        T Convert<T>(object source, Type sourceType);
    }
//    public interface IConverter<T> : IConverter
//    {
//        bool CanConvert<T>(Type targetType);
//        T Convert(object source, Type sourceType);
//    }
}