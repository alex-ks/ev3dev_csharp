using Ev3Dev.CSharp.EvA.AttributeContracts;
using Ev3Dev.CSharp.EvA.Reflection;
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

        public static Delegate[] GetParametersSources(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            var parameters = method.GetParameters();
            var parameterGetters = new Delegate[parameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var parameter = parameters[i];
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

                if (sourceSuspect.Delegate.Method.ReturnType != parameter.ParameterType)
                    throw new InvalidCastException(string.Format(Resources.SourceTypeMismatch,
                                                                 sourceName,
                                                                 parameter.Name,
                                                                 method.Name));

                parameterGetters[i] = sourceSuspect.Delegate;
            }

            return parameterGetters;
        }
    }
}
