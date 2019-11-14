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
    /// Declares that selected method should be called for each loop iteration.
    /// By default, non-async actions will be called synchronously and
    /// do not need to be declared with <see cref="DiscardableAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : Attribute, IActionExtractor
    {
        public Action ExtractAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException(string.Format(Resources.InvalidAction,
                                                                  method.Name));
            var parameters = FromSourceAttribute.GetParametersSources(target, method, properties);
            return DelegateGenerator.GenerateAction(target, method, parameters);
        }

        public Func<Task> ExtractAsyncAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            if (method.ReturnType != typeof(Task))
                throw new InvalidOperationException(string.Format(Resources.InvalidAsyncAction,
                                                                  method.Name));
            var parameters = FromSourceAttribute.GetParametersSources(target, method, properties);
            return DelegateGenerator.GenerateAsyncAction(target, method, parameters);
        }
    }
}
