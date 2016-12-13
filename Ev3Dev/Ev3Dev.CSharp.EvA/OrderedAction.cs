using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    internal struct OrderedAction
    {
        public Action Action { get; }
        public int Priority { get; }

        public OrderedAction( Action action, int priority )
        {
            Action = action;
            Priority = priority;
        }
    }

    internal struct OrderedFunc<T>
    {
        public Func<T> Func { get; }
        public int Priority { get; }

        public OrderedFunc( Func<T> func, int priority )
        {
            Func = func;
            Priority = priority;
        }
    }
}
