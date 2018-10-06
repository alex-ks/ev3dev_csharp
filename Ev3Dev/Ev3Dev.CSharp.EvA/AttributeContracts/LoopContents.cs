using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public class LoopContents
    {
        public IReadOnlyDictionary<string, PropertyPack> Properties { get; set; }
        public IReadOnlyDictionary<string, (Action action, object[] attributes)> Actions { get; set; }
        public IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> AsyncActions { get; set; }
    }
}
