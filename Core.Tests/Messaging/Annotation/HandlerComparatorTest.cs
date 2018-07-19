using System.Collections.Generic;
using FluentAssertions;
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
            _stringHandler = Substitute.For<IMessageHandlingMember>();
            _stringHandler.PayloadType.Returns(typeof(string));
            _stringHandler.Priority.Returns(0);
            
            _objectHandler = Substitute.For<IMessageHandlingMember>();
            _objectHandler.PayloadType.Returns(typeof(object));
            _objectHandler.Priority.Returns(0);
            
            _longHandler = Substitute.For<IMessageHandlingMember>();
            _longHandler.PayloadType.Returns(typeof(long));
            _longHandler.Priority.Returns(0);

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