using System;

namespace Core.Tests.Common
{
    public class MockException : Exception
    {
        public MockException() : base("Mock")
        {
        }

        public MockException(string message) : base(message)
        {
        }
    }
}