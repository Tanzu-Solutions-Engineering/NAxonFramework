using System;

namespace NAxonFramework.Messaging.Attributes
{
    
    public class MetaDataValueAttribute : Attribute
    {
        public MetaDataValueAttribute(string value, bool required = false)
        {
            Value = value;
            Required = required;
        }

        public string Value { get; }
        private bool Required { get; } = false;
    }
}