using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
    public class ShutdownEventAttribute : Attribute
    {

    }
}
