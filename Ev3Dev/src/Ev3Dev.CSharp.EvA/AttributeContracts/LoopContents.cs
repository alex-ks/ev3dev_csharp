using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public struct LoopContents
    {
        public IReadOnlyDictionary<string, PropertyWrapper> Properties { get; }
        public IReadOnlyDictionary<string, (Action action, object[] attributes)> Actions { get; }
        public IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> AsyncActions { get; }

        public LoopContents(IReadOnlyDictionary<string, PropertyWrapper> properties,
                            IReadOnlyDictionary<string, (Action action, object[] attributes)> actions,
                            IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> asyncActions)
        {
            Properties = properties;
            Actions = actions;
            AsyncActions = asyncActions;
        }
    }
}
