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
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShutdownEventAttribute : Attribute
    {

    }
}
