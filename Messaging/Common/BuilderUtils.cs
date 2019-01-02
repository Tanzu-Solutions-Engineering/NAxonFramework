using System;

namespace NAxonFramework.Common
{
    public static class BuilderUtils
    {
        public static void AssertThat<T>(T value, Predicate<T> assertion, string exceptionMessage)
        {
            Assert.AssertThat(value, assertion, () => new AxonConfigurationException(exceptionMessage));
        }
        public static void AssertNonNull<T>(T value, string exceptionMessage)
        {
            AssertThat(value, x => x != null, exceptionMessage);
    }
    }
}