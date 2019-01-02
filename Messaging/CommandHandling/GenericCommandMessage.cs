using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling
{
    public class GenericCommandMessage : MessageDecorator, ICommandMessage
    {
        public string CommandName { get; }


        public static ICommandMessage AsCommandMessage(object command)
        {
            if (command is ICommandMessage message)
            {
                return message;
            }
            return new GenericCommandMessage(command, MetaData.EmptyInstance);
        }

        public GenericCommandMessage(object payload) : this(payload, MetaData.EmptyInstance)
        {
        }

        public GenericCommandMessage(object payload, IReadOnlyDictionary<String, object> metadata) 
            : this(new GenericMessage(payload, metadata), payload.GetType().Name)
        {
            
        }

        public GenericCommandMessage(IMessage @delegate, string commandName) : base(@delegate)
        {
            this.CommandName = commandName;
        }

        public override IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            return new GenericCommandMessage(base.Delegate.WithMetaData(metaData), CommandName);
        }

        public override IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            return new GenericCommandMessage(base.Delegate.AndMetaData(metaData), CommandName);
        }

//        ICommandMessage<T> ICommandMessage<T>.AndMetaData(IReadOnlyDictionary<string, object> metaData) => (ICommandMessage<T>) AndMetaData(metaData);
//
//        ICommandMessage<T> ICommandMessage<T>.WithMetaData(IReadOnlyDictionary<string, object> metaData) => (ICommandMessage<T>) WithMetaData(metaData);

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