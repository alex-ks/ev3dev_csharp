using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Indicates which property will be used to provide value to method argument. This may be useful if you want
    /// to fix property value at the moment of the method launch (for example, if property value may change
    /// during the method execution). In this case, you may use argument value instead of calling property getter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class FromSourceAttribute : Attribute
    {
        public string SourceName { get; }

        public FromSourceAttribute( string sourceName )
        {
            SourceName = sourceName;
        }

        internal static List<Func<object>> GetParametersSources(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, PropertyStorage> properties)
        {
            var parameterGetters = new List<Func<object>>();
            var type = target.GetType();

            foreach (var parameter in method.GetParameters())
            {
                var sourceAttribute = parameter.GetCustomAttribute<FromSourceAttribute>();
                if (sourceAttribute == null)
                    throw new InvalidOperationException(string.Format(Resources.NoParameterSource,
                                                                      parameter.Name,
                                                                      method.Name));

                if (!properties.ContainsKey(sourceAttribute.SourceName))
                    throw new InvalidOperationException(string.Format(Resources.SourceNotFound,
                                                                      sourceAttribute.SourceName,
                                                                      parameter.Name,
                                                                      method.Name));

                var sourceSuspects = properties[sourceAttribute.SourceName];

                if (!sourceSuspects.ContainsKey(parameter.ParameterType))
                    throw new InvalidCastException(string.Format(Resources.SourceTypeMismatch,
                                                                 sourceAttribute.SourceName,
                                                                 parameter.Name,
                                                                 method.Name));

                parameterGetters.Add(sourceSuspects[parameter.ParameterType]);
            }

            return parameterGetters;
        }
    }
}
