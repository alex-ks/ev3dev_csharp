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
    /// Declares that selected method should be called for each loop iteration.
    /// By default, non-async actions will be called synchronously and 
    /// do not need to be declared with <see cref="NonReenterableAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : Attribute, IActionExtractor
    {
        public Action ExtractAction(
            object target, 
            MethodInfo method, 
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
        {
            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException(string.Format(Resources.InvalidAction,
                                                                  method.Name));

            var parameters = GetParametersSources(target, method, properties);

            Action<object[]> callAction = DelegateGenerator.GenerateAction(target, method);

            Action performAction = () =>
            {
                var argumentsArray = parameters.Select(getter => getter()).ToArray();
                callAction(argumentsArray);
            };

            return performAction;
        }

        public Func<Task> ExtractAsyncAction(
            object target, 
            MethodInfo method, 
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
        {
            if (method.ReturnType != typeof(Task))
                throw new InvalidOperationException(string.Format(Resources.InvalidAsyncAction,
                                                                  method.Name));

            var parameters = GetParametersSources(target, method, properties);

            Func<object[], Task> callAction = DelegateGenerator.GenerateAsyncAction(target, method);

            Func<Task> performAction = () =>
            {
                var argumentsArray = parameters.Select(getter => getter()).ToArray();
                return callAction(argumentsArray);
            };

            return performAction;
        }

        private List<Func<object>> GetParametersSources(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
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
