using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;
using NSubstitute;
using Xunit;

namespace Core.Tests.Messaging.Annotation
{
    public class HandlerComparatorTest
    {
        private readonly IMessageHandlingMember _stringHandler;
        private readonly IComparer<IMessageHandlingMember> _testSubject;
        private readonly IMessageHandlingMember _objectHandler;
        private readonly IMessageHandlingMember _longHandler;

        public HandlerComparatorTest()
        {
            IMessageHandlingMember CreateMock<T>()
            {
                var mock = Substitute.For<IMessageHandlingMember>();
                mock.Priority.Returns(0);
                mock.PayloadType.Returns(typeof(T));
                mock.Unwrap<MethodBase>().Returns(Optional<MethodBase>.Empty);
                return mock;
            }

            _stringHandler = CreateMock<string>();
            _objectHandler = CreateMock<object>();
            _longHandler = CreateMock<long>();

            _testSubject = HandlerComparator.Instance;
        }
        
        [Fact]
        public void TestSubclassesBeforeSuperclasses()
        {
            _testSubject.Compare(_stringHandler, _objectHandler).Should().BeNegative("String should appear before Object");
            _testSubject.Compare(_objectHandler, _stringHandler).Should().BePositive("String should appear before Object");
            
            
            _testSubject.Compare(_longHandler, _objectHandler).Should().BeNegative("Long should appear before Object");
            _testSubject.Compare(_objectHandler, _longHandler).Should().BePositive("Long should appear before Object");
            
        }
        
        [Fact]
        public void TestHandlersIsEqualWithItself()
        {
            _testSubject.Compare(_stringHandler, _stringHandler).Should().Be(0);
            _testSubject.Compare(_objectHandler, _objectHandler).Should().Be(0);
            _testSubject.Compare(_longHandler, _longHandler).Should().Be(0);
            
            _testSubject.Compare(_stringHandler, _objectHandler).Should().NotBe(0);
            _testSubject.Compare(_longHandler, _stringHandler).Should().NotBe(0);
            _testSubject.Compare(_objectHandler, _longHandler).Should().NotBe(0);
        }

        [Fact]
        public void TestHandlersSortedCorrectly()
        {
            var members = new List<IMessageHandlingMember> { _objectHandler, _stringHandler, _longHandler};
            
            members.Sort(_testSubject);
            //todo: no numbers inheritance in .net, possibly create different test case to handle inheritance rules
            members.Should().HaveElementAt(2, _objectHandler);
        }
        
    }
}