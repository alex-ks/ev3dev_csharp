using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EverythingIsNonCriticalAttribute : Attribute, ILoopTransformer
    {
        public LoopContents TransformLoop(LoopContents contents, object[] loopAttributes)
        {
            // todo: add message to resources
            if (loopAttributes.Where(attr => attr is EverythingIsNonCriticalAttribute).FirstOrDefault() != null)
                throw new InvalidOperationException("Ambiguous definition: EverythingIsNonCriticalAttribute is set too");
            throw new NotImplementedException();
        }
    }
}
