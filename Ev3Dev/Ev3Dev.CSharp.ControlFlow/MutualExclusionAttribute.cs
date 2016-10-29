using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public class MutualExclusionAttribute : Attribute
    {
        public string[] Methods { get; }

        public MutualExclusionAttribute( params string[] methods )
        {
            Methods = methods;
        }
    }
}
