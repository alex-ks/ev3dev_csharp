using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    // It's guaranteered that properties won't be called simultaneously, so there is no need to make them reenterable.
    /// <summary>
    /// Declares that method is not reenterable. All method calls performed while another call being executed
    /// will be discarded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DiscardableAttribute : AbstractSynchronizedTransformer
    {
        protected sealed override Action TransformActionImpl(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            return () =>
            {
                if (!Monitor.TryEnter(LockGuard))
                    return;
                try { action(); }
                finally { Monitor.Exit(LockGuard); }
            };
        }

        protected sealed override Func<Task> TransformAsyncActionImpl(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            return async () =>
            {
                lock (LockGuard)
                {
                    if (LockGuard.IsLocked)
                        return;
                    LockGuard.IsLocked = true;
                }
                try { await action(); }
                finally
                {
                    lock (LockGuard)
                    {
                        LockGuard.IsLocked = false;
                        Monitor.Pulse(LockGuard);
                    }
                }
            };
        }
    }
}
