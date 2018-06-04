using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    // its guaranteered that properties won't be called simultaneously, so there is no need to make them reenterable
    /// <summary>
    /// Declares that method is not reenterable. For default model building (<see cref="ModelParser"/>) only makes 
    /// sense for async methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NonReenterableAttribute : Attribute, IActionTransformer
    {
        public bool DiscardRepeated { get; set; } = true;

        public Action TransformAction(string name, Action action, object[] attributes, IReadOnlyDictionary<string, PropertyStorage> properties)
        {
            var lockGuard = new object();

            if (DiscardRepeated)
                return () =>
                {
                    if (!Monitor.TryEnter(lockGuard))
                        return;

                    try { action(); }
                    finally { Monitor.Exit(lockGuard); }
                };

            return () => { lock (lockGuard) { action(); } };
        }

        public Func<Task> TransformAsyncAction(string name, Func<Task> action, object[] attributes, IReadOnlyDictionary<string, PropertyStorage> properties)
        {
            var lockGuard = new object();

            // check correctness with monitor
            // looks like invalid
            if (DiscardRepeated)
                return async () =>
                {
                    if (!Monitor.TryEnter(lockGuard))
                        return;

                    try { await action(); }
                    finally { Monitor.Exit(lockGuard); }
                };

            throw new NotImplementedException();
        }
    }
}
