using System;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    public sealed class HandlerComparator
    {
        private static IComparer<IMessageHandlingMember> INSTANCE
            = Comparer<IMessageHandlingMember>.Default
                .Comparing(x => x.PayloadType, Comparer<Type>.Create(CompareHierarchy))
                .ThenComparing(x => x.Priority * -1)
                .ThenComparing(m => m.Unwrap<MethodBase>()?.Name ?? m.ToString());
        // prevent construction
        private HandlerComparator() {
        }

        /**
         * Returns the singleton comparator managed by the HandlerComparator class.
         *
         * @return the singleton comparator
         */
        public static IComparer<IMessageHandlingMember> Instance => INSTANCE;

        private static int CompareHierarchy(Type o1, Type o2) {
            if (o1 == o2) {
                return 0;
            } else if (o1.IsAssignableFrom(o2)) {
                return 1;
            } else if (o2.IsAssignableFrom(o1)) {
                return -1;
            }
            return depthOf(o2).CompareTo(depthOf(o1));
        }

        private static int depthOf(Type o1) {
            int depth = 0;
            Type type = o1;
            while (type != null && type != typeof(object)) {
                depth++;
                type = type.BaseType;
            }
            if (o1.IsAttribute()) {
                depth += 1000;
            }
            return depth;
        }

    }
}