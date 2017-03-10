using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Indicates which property will be used to provide value to method argument. This may be useful if you want
    /// to fix property value at the moment of the method launch (for example, if property value may change
    /// during the method execution). In this case, you may use argument value instead of calling property getter.
    /// </summary>
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
