using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Declares thar any exception in the selected method will not take any effect
    /// on program execution. Any exception will be logged.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class NonCriticalAttribute : Attribute
    {
        public bool LogExceptions { get; set; } = true;
    }
}
