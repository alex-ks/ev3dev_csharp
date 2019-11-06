using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Sets the method priority for event loop.
    /// The less priority number, the higher priority, the earlier loop will call action or check event
    ///  handler conditions.
    /// Combine with <see cref="MutualExclusionAttribute"/>, if two mutexed methods can be called simultaneously,
    /// method with higher priority will be called first.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class PriorityAttribute : Attribute
    {
        public int Priority { get; }

        public PriorityAttribute( int priority )
        {
            Priority = priority;
        }
    }
}
