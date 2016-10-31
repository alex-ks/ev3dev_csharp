using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    // its guaranteered that properties won't be called simultaneously, so there is no need to make them reenterable
    /// <summary>
    /// Declares that method is not reenterable. For default model building (<see cref="ModelParser"/>) only makes 
    /// sense for async methods.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class NonReenterableAttribute : Attribute
    {
        public bool DiscardRepeated { get; set; } = true;
    }
}
