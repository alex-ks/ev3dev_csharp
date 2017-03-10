using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Indicates that marked property can be used as <see cref="bool"/> value.
    /// In this case, property value will be true if property value has changed since last check,
    /// and false ortherwise.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
    public class SwitchAttribute : Attribute
    {
        
    }
}
