using System;
using Xunit;
using FluentAssertions;
using NAxonFramework.Common;
using NAxonFramework.Common.Attributes;

namespace Core.Tests.Common.Annotation
{
    public class AnnotationUtilsTest
    {
        [Fact]
        public void TestFindAttributesOnDirectAnnotation()
        {
            var results = AnnotationUtils.FindAnnotationAttributes(GetType().GetMethod(nameof(DirectAnnotated)), typeof(TheTarget)).Get();

            results["Property"].Should().Be("value");
            results.ContainsKey("value").Should().BeFalse("value property should use annotation Simple class name as key");
            results["TheTarget"].Should().Be("value()");
        }
        [Fact]
        public void TestFindAttributesOnStaticMetaAnnotationConstructor()
        {
            var results = AnnotationUtils.FindAnnotationAttributes(GetType().GetMethod(nameof(StaticallyOverriddenConstructor)), typeof(TheTarget)).Get();
            results["Property"].Should().Be("overridden_statically");
        }
        [Fact]
        public void TestFindAttributesOnStaticMetaAnnotationNamed()
        {
            var results = AnnotationUtils.FindAnnotationAttributes(GetType().GetMethod(nameof(StaticallyOverriddenNamedProperty)), typeof(TheTarget)).Get();
            results["Property"].Should().Be("overridden_statically");
        }
        [Fact]
        public void TestFindAttributesOnDynamicMetaAnnotation()
        {
            var results = AnnotationUtils.FindAnnotationAttributes(GetType().GetMethod(nameof(DynamicallyOverridden)), typeof(TheTarget)).Get();
            results["Property"].Should().Be("dynamic-override");
            results.GetValueOrDefault("ExtraValue").Should().Be("extra");
        }       
       

        [TheTarget]
        public void DirectAnnotated()
        {
            
        }
        
        [StaticOverrideConstructorAttribute]
        public void StaticallyOverriddenConstructor()
        {
            
        }
        [StaticOverrideNamedArgumentAttribute]
        public void StaticallyOverriddenNamedProperty()
        {
            
        }
        [DynamicOverrideAttribute(property: "dynamic-override")]
        public void DynamicallyOverridden()
        {
            
        }

        [TheTarget(property: "overridden_statically")]
        public class StaticOverrideConstructorAttribute : Attribute
        {
            
        }

        public class TheTarget : Attribute
        {
            public TheTarget(string property = "value", string value = "value()")
            {
                Property = property;
                Value = value;
            }

            public string Property { get; set; }
            public string Value { get; set; }
        }

        [TheTarget(Property = "overridden_statically")]
        public class StaticOverrideNamedArgumentAttribute : Attribute
        {
            
        }

        [TheTarget]
        public class DynamicOverrideAttribute : Attribute
        {
            public DynamicOverrideAttribute(string property = null, string extraValue = "extra", string theTarget = "otherValue")
            {
                Property = property;
                ExtraValue = extraValue;
                TheTarget = theTarget;
            }

            public string Property { get; set; }
            public string ExtraValue { get; set; }
            public string TheTarget { get; set; }
        }
    }
}