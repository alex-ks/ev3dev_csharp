using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    /// <summary>
    /// Declares that selected method should be called for each loop iteration.
    /// By default, non-async actions will be called synchronously and 
    /// do not need to be declared with <see cref="NonReenterableAttribute"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class ActionAttribute : Attribute
    {
        
    }
}
