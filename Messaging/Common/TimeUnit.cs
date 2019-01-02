using System;

namespace NAxonFramework.Common
{
    public enum TimeUnit
    {
        MilliSeconds,
        Seconds,
        Minutes,
        Hours,
        Days
    }

    public static class TimeUnitExtensions
    {
        public static TimeSpan ToTimeSpan(this TimeUnit timeUnit, long value)
        {
            switch (timeUnit)
            {
                case TimeUnit.MilliSeconds:
                    return TimeSpan.FromMilliseconds(value);
                case TimeUnit.Seconds:
                    return TimeSpan.FromSeconds(value);
                case TimeUnit.Minutes:
                    return TimeSpan.FromMinutes(value);
                case TimeUnit.Hours:
                    return TimeSpan.FromHours(value);
                case TimeUnit.Days:
                    return TimeSpan.FromDays(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit), timeUnit, null);
            }
        }
    }
}