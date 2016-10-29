using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    public enum CompositionType
    {
        AND, OR
    }

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class EventHandlerAttribute : Attribute
    {
        public string[] Triggers { get; }

        public CompositionType TriggerComposition { get; }

        public EventHandlerAttribute( params string[] triggers )
        {
            Triggers = triggers;
            TriggerComposition = CompositionType.AND;
        }

        public EventHandlerAttribute( CompositionType type, params string[] triggers )
        {
            Triggers = triggers;
            TriggerComposition = type;
        }
    }
}
