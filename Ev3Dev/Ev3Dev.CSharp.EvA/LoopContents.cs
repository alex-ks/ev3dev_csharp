using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    internal struct LoopContents
    {
        internal IEnumerable<(Action action, object[] attributes)> Actions { get; }
        internal object[] ModelAttributes { get; }

        internal LoopContents(IEnumerable<(Action action, object[] attributes)> actions, object[] modelAttributes)
        {
            Actions = actions;
            ModelAttributes = modelAttributes;
        }       
    }

    internal static class LoopContentsConverter
    {
        internal static LoopContents ToLoopContents(this ActionContents contents, object[] modelAttributes)
        {
            var flattenAsyncs = contents.AsyncActions.Values.Select(t =>
            {
                Action action = () => t.action();
                return (action, t.attributes);
            });

            var actions = flattenAsyncs.Concat(contents.Actions.Values);
            return new LoopContents(actions, modelAttributes);
        }
    }
}
