using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.EventHandling;
using NAxonFramework.EventSourcing;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public static class Conflicts
    {
        public static Predicate<List<T>> EventMatching<T>(Predicate<T> messageFilter)  where T : IEventMessage
        {
            return events => events.Any(x => messageFilter(x));
        }
        
        public static Predicate<List<IDomainEventMessage>> PayloadMatching(Predicate<Object> payloadFilter) 
        {
            return events => events.Select(x => x.Payload).Any(x => payloadFilter(x));
        }
        public static Predicate<List<IDomainEventMessage>> PayloadMatching<T>(Predicate<T> payloadFilter) 
        {
            return events => events.Where(e => typeof(T).IsAssignableFrom(e.PayloadType))
                .Select(e => (T) e.Payload).Any(x => payloadFilter(x));
        }
        public static Predicate<List<IDomainEventMessage>> PayloadTypeOf<T>() where T : IEventMessage
        {
            return EventMatching<IDomainEventMessage>(e => typeof(T).IsAssignableFrom(e.PayloadType));
            
        }
    }
}