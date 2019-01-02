using System;
using System.Threading.Tasks;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public class TrackingEventProcessorConfiguration
    {
        private const int DEFAULT_BATCH_SIZE = 1;
        private const int DEFAULT_THREAD_COUNT = 1;
        object x = TaskScheduler.Current
        private readonly int _maxThreadCount;
        private int _batchSize;
        private int _initialSegmentCount;
        private Func<IStreamableMessageSource<IMessage>, ITrackingToken> _initialTrackingTokenBuilder = _ => null;
        private Func<string, IThreadFactory> threadFactory;
    }
}