using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling
{
    public class GenericCommandMessage<T> : MessageDecorator<T>, ICommandMessage<T>
    {
        public string CommandName { get; }


        public static ICommandMessage<T> AsCommandMessage(object command)
        {
            if (command is ICommandMessage<T> message)
            {
                return message;
            }
            return new GenericCommandMessage<T>((T)command, MetaData.EmptyInstance);
        }

        public GenericCommandMessage(T payload) : this(payload, MetaData.EmptyInstance)
        {
        }

        public GenericCommandMessage(T payload, IReadOnlyDictionary<String, object> metadata) : this(new GenericMessage<T>(payload, metadata), payload.GetType().Name)
        {
            
        }

        public GenericCommandMessage(IMessage<T> @delegate, string commandName) : base(@delegate)
        {
            this.CommandName = commandName;
        }

        public override IMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            return new GenericCommandMessage<T>(base.Delegate.WithMetaData(metaData), CommandName);
        }

        public override IMessage<T> AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            return new GenericCommandMessage<T>(base.Delegate.AndMetaData(metaData), CommandName);
        }

        ICommandMessage<T> ICommandMessage<T>.AndMetaData(IReadOnlyDictionary<string, object> metaData) => (ICommandMessage<T>) AndMetaData(metaData);

        ICommandMessage<T> ICommandMessage<T>.WithMetaData(IReadOnlyDictionary<string, object> metaData) => (ICommandMessage<T>) WithMetaData(metaData);

        protected override void DescribeTo(StringBuilder stringBuilder)
        {
            base.DescribeTo(stringBuilder);
            stringBuilder.Append(", commandName='")
                .Append(CommandName)
                .Append('\'');
        }

        protected override string DescribeType => "GenericCommandMessage";
    }
}