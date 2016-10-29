using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class ActionAttribute : Attribute
    {
        
    }
}
