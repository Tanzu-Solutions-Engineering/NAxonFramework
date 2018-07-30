using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoreLinq.Extensions;

namespace NAxonFramework.Common.Attributes
{
    //todo: modify to take into account declared attributes that are not passed in as parameter or named attributes. should read defaults by constructing a single instance at runtime and caching its values
    public abstract class AnnotationUtils
    {
        public static bool IsAttributePresent<T>(ICustomAttributeProvider element) where T : Attribute 
        {
            return element.HasAttribute<T>();
        } 
        
        public static Optional<Dictionary<String, Object>> FindAnnotationAttributes<T>(ICustomAttributeProvider element) where T : Attribute
        {
            return FindAnnotationAttributes(element, typeof(T));
        }
        
        public static Optional<Dictionary<String, Object>> FindAnnotationAttributes(ICustomAttributeProvider element, Type annotationType)
        {
            Dictionary<String, Object> result;
            switch (element)
            {
                case MemberInfo memberInfo:
                    result = Scan(memberInfo, annotationType, new HashSet<Type>());
                    break;
                case ParameterInfo parameterInfo:
                    result = Scan(parameterInfo, annotationType, new HashSet<Type>());
                    break;
                default:
                    throw new ArgumentException($"{element.GetType()} is not a supported attribute provider", nameof(element));
            }

            return result != null ? Optional<Dictionary<string, object>>.Of(result) : Optional<Dictionary<string, object>>.Empty;
        }
        
        
        static Dictionary<string, object> Scan(ParameterInfo target, Type attributeType, HashSet<Type> visited)
        {
            var data = target.GetCustomAttributesData();
            return Scan(data, attributeType, visited);
        }
        static Dictionary<string,object> Scan(MemberInfo target, Type attributeType, HashSet<Type> visited)
        {
            if (visited.Contains(target)) return null;
            var data = target.GetCustomAttributesData();
            return Scan(data, attributeType, visited);
        }
        static Dictionary<string,object> Scan(IList<CustomAttributeData> data, Type attributeType, HashSet<Type> visited)
        {

            var targetData = data.FirstOrDefault(x => x.AttributeType == attributeType);
            if(targetData != null)
            {
                return ToData(targetData);
            }
            else
            {
                var scanResult = data.Select(x => new { Parent = x.AttributeType, ChildData = Scan(x.AttributeType, attributeType, visited)}).FirstOrDefault(x => x.ChildData != null);
                if (scanResult == null) return null;
                var result = scanResult.ChildData;
                if(scanResult.ChildData.Any())
                {
                    var parentData = ToData(data.First(x => x.AttributeType == scanResult.Parent));
				
                    foreach(var key in parentData.Keys)
                    {
                        result[key] = parentData[key];
                    }
                }
                return result;
            }
        }
        static Dictionary<string,object> ToData(CustomAttributeData data)
        {
            
            //return 	data.NamedArguments.ToDictionary(x => x.MemberName, x => x.TypedValue.Value);
            return data.Constructor
                .GetParameters()
                .Select(x => x.Name)
                .Zip(data.ConstructorArguments.Select(x => x.Value), (name, value) => new { name, value })
                .FullGroupJoin(data.NamedArguments, 
                    x => x.name.First().ToString().ToUpper() + x.name.Substring(1),
                    x => x.MemberName, 
                    (name, ctr, prm) => new { Key = name.ToLower() != "value" ? name : data.AttributeType.Name, Value = prm.Select(x => x.TypedValue.Value).DefaultIfEmpty(ctr.Select(x => x.value).FirstOrDefault()).FirstOrDefault() })
                .ToDictionary(x => x.Key, x => x.Value);;
        }
        //private static string ResolveName()
    }
}