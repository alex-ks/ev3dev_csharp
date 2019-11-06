using Ev3Dev.CSharp.EvA.AttributeContracts;
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
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            var parameterGetters = new List<Func<object>>();
            var type = target.GetType();

            foreach (var parameter in method.GetParameters())
            {
                var sourceAttribute = parameter.GetCustomAttribute<FromSourceAttribute>();
                string sourceName = null;

                if (sourceAttribute == null)
                {
                    sourceName = parameter.Name;
                    sourceName = string.Concat(
                            sourceName.Take(1)
                                      .Select(char.ToUpper)
                                      .Concat(sourceName.Skip(1)));
                }
                else
                {
                    sourceName = sourceAttribute.SourceName;
                }

                if (!properties.ContainsKey(sourceName))
                    throw new InvalidOperationException(string.Format(Resources.SourceNotFound,
                                                                      sourceName,
                                                                      parameter.Name,
                                                                      method.Name));

                var sourceSuspect = properties[sourceName];

                if (sourceSuspect.Type != parameter.ParameterType)
                    throw new InvalidCastException(string.Format(Resources.SourceTypeMismatch,
                                                                 sourceName,
                                                                 parameter.Name,
                                                                 method.Name));

                parameterGetters.Add(sourceSuspect.GenericGetter);
            }

            return parameterGetters;
        }
    }
}
