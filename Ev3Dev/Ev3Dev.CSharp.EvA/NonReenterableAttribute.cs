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

        private class NonReenterableAsync
        {
            private volatile bool _locked = false;
            private object _lockGuard = new object();
            private Func<Task> _action;

            internal NonReenterableAsync(Func<Task> action)
            {
                _action = action;
            }

            internal async Task InvokeDiscardingRepeated()
            {
                lock (_lockGuard)
                {
                    if (_locked)
                        return;
                    _locked = true;
                }
                try { await _action(); }
                finally { lock (_lockGuard) { _locked = false; } }
            }

            internal async Task InvokeCumulative()
            {
                lock (_lockGuard)
                {
                    while (_locked)
                        Monitor.Wait(_lockGuard);
                    _locked = true;
                }
                try { await _action(); }
                finally { lock (_lockGuard) { _locked = false; } }
            }
        }

        public Func<Task> TransformAsyncAction(string name, Func<Task> action, object[] attributes, IReadOnlyDictionary<string, PropertyStorage> properties)
        {
            var guarded = new NonReenterableAsync(action);

            if (DiscardRepeated)
                return guarded.InvokeDiscardingRepeated;

            return guarded.InvokeCumulative;
        }
    }
}
