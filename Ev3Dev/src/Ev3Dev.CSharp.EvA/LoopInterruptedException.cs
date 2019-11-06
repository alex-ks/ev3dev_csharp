using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    public class LoopInterruptedException : AggregateException
    {
        // todo: add message to resources
        public LoopInterruptedException() : base("Loop was interrupted and must be stopped") { }

        public LoopInterruptedException(string message) : base(message) { }

        public LoopInterruptedException(params Exception[] innerExceptions) : base(innerExceptions) { }
    }
}
