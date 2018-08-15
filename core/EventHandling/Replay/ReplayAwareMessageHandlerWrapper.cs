using System;
using System.Collections.Generic;
using NAxonFramework.Common;
using NAxonFramework.Common.Attributes;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Replay
{
    public class ReplayAwareMessageHandlerWrapper : IHandlerEnhancerDefinition
    {
        static Dictionary<string, object> DefaultSetting = new Dictionary<string, object>() {{"allowReplay", true}};

        public IMessageHandlingMember WrapHandler(IMessageHandlingMember original)
        {
            var isReplayAllowed = (bool) original
                .AnnotationAttributes(typeof(AllowReplayAttribute))
                .OrElseGet(() => Optional<Type>.OfNullable(original.GetType().DeclaringType)
                    .Map(c => AnnotationUtils.FindAnnotationAttributes(c, typeof(AllowReplayAttribute))
                        .OrElse(DefaultSetting))
                    .OrElse(DefaultSetting)
                )["allowReplay"];

            if (!isReplayAllowed)
            {
                return new ReplayBlockingMessageHandlingMember(original);
            }

            return original;
        }

        public IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original)
        {
            throw new NotImplementedException();
        }

        private class ReplayBlockingMessageHandlingMember : WrappedMessageHandlingMember
        {
            public ReplayBlockingMessageHandlingMember(IMessageHandlingMember original) : base(original)
            {
            }

            public override object Handle(IMessage message, object target)
            {
                if (ReplayToken.IsReplay(message))
                {
                    return null;
                }

                return base.Handle(message, target);
            }
        }
    }
}