using System.Reflection;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.Attributes;
using FluentAssertions;
using NAxonFramework.CommandHandling;
using Xunit;

namespace Core.Tests.Messaging.Annotation
{
    public class MessageIdentifierParameterResolverFactoryTest
    {
        private MethodInfo _messageIdentifierMethod;
        private MethodInfo _nonAnnotatedMethod;
        private MethodInfo _integerMethod;
        private MessageIdentifierParameterResolverFactory _testSubject;

        public MessageIdentifierParameterResolverFactoryTest()
        {
            _testSubject = new MessageIdentifierParameterResolverFactory();

            _messageIdentifierMethod = this.GetType().GetMethod(nameof(SomeMessageIdentifierMethod));
            _nonAnnotatedMethod = this.GetType().GetMethod(nameof(SomeNonAnnotatedMethod));
            _integerMethod = this.GetType().GetMethod(nameof(SomeIntegerMethod));
        }
        
        public void SomeMessageIdentifierMethod([MessageIdentifier] string messageIdentifier) {
        }

        public void SomeNonAnnotatedMethod(string messageIdentifier) {
        }

        public void SomeIntegerMethod([MessageIdentifier] int messageIdentifier) {
        }

        [Fact]
        public void TestResolvesToMessageIdentifierWhenAnnotatedForEventMessage() {
            IParameterResolver resolver = _testSubject.CreateInstance(_messageIdentifierMethod, _messageIdentifierMethod.GetParameters(), 0);
            var eventMessage = GenericEventMessage<object>.AsEventMessage("test");
            resolver.Matches(eventMessage).Should().BeTrue();
            resolver.ResolveParameterValue(eventMessage).Should().Be(eventMessage.Identifier);
        }
        
        [Fact]
        public void TestResolvesToMessageIdentifierWhenAnnotatedForCommandMessage() {
            IParameterResolver resolver = _testSubject.CreateInstance(_messageIdentifierMethod, _messageIdentifierMethod.GetParameters(), 0);
            var eventMessage = GenericCommandMessage<object>.AsCommandMessage("test");
            resolver.Matches(eventMessage).Should().BeTrue();
            resolver.ResolveParameterValue(eventMessage).Should().Be(eventMessage.Identifier);
        }
        
        [Fact]
        public void TestIgnoredWhenNotAnnotated() {
            IParameterResolver resolver = _testSubject.CreateInstance(_nonAnnotatedMethod, _nonAnnotatedMethod.GetParameters(), 0);
            resolver.Should().BeNull();
        }
        [Fact]
        public void TestIgnoredWhenWrongType() {
            IParameterResolver resolver = _testSubject.CreateInstance(_integerMethod, _integerMethod.GetParameters(), 0);
            resolver.Should().BeNull();
        }
    }
}