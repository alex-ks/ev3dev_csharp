using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Declares that <see cref="EventLoop"/> will be stopped if the specified boolean
    /// property becomes true. All shutdown events are checked *in the beginning* of a loop iteration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomShutdownEventAttribute : Attribute
    {
        internal string Name { get; private set; }

        public CustomShutdownEventAttribute(string name)
        {
            if (name == null)
                throw new ArgumentException("Shutdown event name cannot be null.");  // todo: add to resources.
            Name = name;
        }
    }
}
