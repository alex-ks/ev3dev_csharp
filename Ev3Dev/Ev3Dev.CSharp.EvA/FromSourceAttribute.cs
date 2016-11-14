using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage( AttributeTargets.Parameter, AllowMultiple = true )]
    public class FromSourceAttribute : Attribute
    {
        public string SourceName { get; }

        public FromSourceAttribute( string sourceName )
        {
            SourceName = sourceName;
        }
    }
}
