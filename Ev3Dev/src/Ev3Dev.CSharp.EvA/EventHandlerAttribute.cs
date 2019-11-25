using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ev3Dev.CSharp.EvA.AttributeContracts;
using Ev3Dev.CSharp.EvA.Reflection;

namespace Ev3Dev.CSharp.EvA
{
    public enum CompositionType
    {
        And, Or
    }

    /// <summary>
    /// Declares that method will be called only if condition in attribute costructor (boolean property
    /// or property marked with <see cref="SwitchAttribute"/>) is satisfied. You can set multiple conditions and type of their composition (boolean AND/OR).
    /// Default composition type is <see cref="CompositionType.And"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventHandlerAttribute : Attribute, IActionExtractor
    {
        public string[] Triggers { get; }

        public CompositionType TriggerComposition { get; }

        /// <summary>
        /// Declares that method will be called only if passed conditions (triggers) are satisfied.
        /// </summary>
        /// <param name="triggers">
        /// Names of boolean property or property marked with <see cref="SwitchAttribute"/>.
        /// </param>
        public EventHandlerAttribute(params string[] triggers)
        {
            Triggers = triggers;
            TriggerComposition = CompositionType.And;
        }

        /// <summary>
        /// Declares that method will be called only if passed conditions (triggers) are satisfied.
        /// </summary>
        /// <param name="type">Determines how triggers will be composed.</param>
        /// <param name="triggers">Names of boolean property or switch.</param>
        public EventHandlerAttribute(CompositionType type, params string[] triggers)
        {
            Triggers = triggers;
            TriggerComposition = type;
        }

        public Action ExtractAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException(string.Format(Resources.InvalidAction,
                                                                  method.Name));

            if (Triggers.Length == 0)
                throw new InvalidOperationException(string.Format(Resources.InvalidEventTriggerCount,
                                                                  method.Name));

            var parameters = FromSourceAttribute.GetParametersSources(target, method, properties);
            var composedTrigger = ComposeTrigger(method.Name, properties);

            Action callAction = DelegateGenerator.GenerateAction(target, method, parameters);

            Action performAction = () =>
            {
                if (composedTrigger())
                    callAction();
            };

            return performAction;
        }

        public Func<Task> ExtractAsyncAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            if (method.ReturnType != typeof(Task))
                throw new InvalidOperationException(string.Format(Resources.InvalidAsyncAction,
                                                                  method.Name));

            if (Triggers.Length == 0)
                throw new InvalidOperationException(string.Format(Resources.InvalidEventTriggerCount,
                                                                  method.Name));

            var parameters = FromSourceAttribute.GetParametersSources(target, method, properties);
            var composedTrigger = ComposeTrigger(method.Name, properties);

            Func<Task> callAction = DelegateGenerator.GenerateAsyncAction(target, method, parameters);

            Func<Task> performAction = () =>
            {
                if (composedTrigger())
                    return callAction();
                return Task.CompletedTask;
            };

            return performAction;
        }

        private Func<bool> ComposeTrigger(
            string methodName,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            var triggers = Triggers.Select(name => properties[name].Delegate as Func<bool>).ToList();
            if (triggers.Any(t => t == null))
                throw new InvalidOperationException("Some triggers are not boolean getters.");

            Func<bool, bool, bool> compositionFunc;
            if (TriggerComposition == CompositionType.And)
                compositionFunc = (a, b) => a && b;
            else if (TriggerComposition == CompositionType.Or)
                compositionFunc = (a, b) => a || b;
            else
                throw new InvalidOperationException(string.Format(Resources.UnknownTriggerComposition,
                                                                  methodName));

            return () => triggers.Select(t => t()).Aggregate(compositionFunc);
        }
    }
}
