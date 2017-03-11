using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class EventHandlerAttribute : Attribute
    {
        public string[] Triggers { get; }

        public CompositionType TriggerComposition { get; }

        /// <summary>
        /// Declares that method will be called only if passed conditions (triggers) are satisfied.
        /// </summary>
        /// <param name="triggers">
        /// Names of boolean property or property marked with <see cref="SwitchAttribute"/>.
        /// </param>
        public EventHandlerAttribute( params string[] triggers )
        {
            Triggers = triggers;
            TriggerComposition = CompositionType.And;
        }

        /// <summary>
        /// Declares that method will be called only if passed conditions (triggers) are satisfied.
        /// </summary>
        /// <param name="type">Determines how triggers will be composed.</param>
        /// <param name="triggers">Names of boolean property or switch.</param>
        public EventHandlerAttribute( CompositionType type, params string[] triggers )
        {
            Triggers = triggers;
            TriggerComposition = type;
        }
    }
}
