using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Declares that <see cref="EventLoop"/> will be stopped if this condition (boolean property
    /// or property marked with <see cref="SwitchAttribute"/>) is true.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
    public class ShutdownEventAttribute : Attribute
    {

    }
}
