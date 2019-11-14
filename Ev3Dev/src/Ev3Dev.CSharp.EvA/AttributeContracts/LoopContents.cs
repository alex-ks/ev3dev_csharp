using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ev3Dev.CSharp.EvA.Reflection;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public struct LoopContents
    {
        public IReadOnlyDictionary<string, ICachingDelegate> Properties { get; }
        public IReadOnlyDictionary<string, (Action action, object[] attributes)> Actions { get; }
        public IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> AsyncActions { get; }

        public LoopContents(IReadOnlyDictionary<string, ICachingDelegate> properties,
                            IReadOnlyDictionary<string, (Action action, object[] attributes)> actions,
                            IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> asyncActions)
        {
            Properties = properties;
            Actions = actions;
            AsyncActions = asyncActions;
        }
    }
}
