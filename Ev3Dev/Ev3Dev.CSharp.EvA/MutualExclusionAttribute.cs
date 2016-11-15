using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public class MutualExclusionAttribute : Attribute
    {
        public string[] Methods { get; }

        /// <summary>
        /// Indicates whether to discard action or event handler if 
        /// one of the mutexed methods is running.
        /// </summary>
        public bool DiscardExcluded { get; set; } = false;

        public MutualExclusionAttribute( params string[] methods )
        {
            Methods = methods;
        }
    }
}
