using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
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
