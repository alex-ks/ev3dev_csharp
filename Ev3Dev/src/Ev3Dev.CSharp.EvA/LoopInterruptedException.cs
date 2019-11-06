using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Signalizes that loop execution should be stopped immediately.
    /// </summary>
    public class LoopInterruptedException : AggregateException
    {
        // todo: add message to resources
        public LoopInterruptedException() : base("Loop was interrupted and must be stopped") { }

        public LoopInterruptedException(string message) : base(message) { }

        public LoopInterruptedException(params Exception[] innerExceptions) : base(innerExceptions) { }
    }
}
