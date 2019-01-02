using System;

namespace NAxonFramework.Common
{
    public static class Assert
    {
        public static void State(bool state, Func<string> messageSupplier)
        {
            if(!state)
                throw new InvalidOperationException(messageSupplier());
        }
        public static void IsTrue(bool expression, Func<string> messageSupplier)
        {
            if(!expression)
                throw new InvalidOperationException(messageSupplier());
        }
        public static void IsFalse(bool expression, Func<string> messageSupplier)
        {
            if(expression)
                throw new InvalidOperationException(messageSupplier());
        }
        public static void NotNull(object value, Func<string> messageSupplier)
        {
            IsTrue(value != null, messageSupplier);
        }
        
        public static T NotNull<T>(T value, Func<string> messageSupplier)
        {
            IsTrue(value != null, messageSupplier);
            return value;
        }
    }
}