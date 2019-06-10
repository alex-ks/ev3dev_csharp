using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    /// <summary>
    /// Declares that method is not reenterable. If the method is called while another call is being executed, the loop
    /// will wait for previous method finish.
    /// When using property forwarding, the properties will be took from the current iteration scope, so nothing is
    /// changed compared to synchronous actions.
    /// </summary>
    public class AwaitableAttribute : AbstractSynchronizedTransformer
    {
        protected sealed override Action TransformActionImpl(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties)
        {
            return () => { lock (LockGuard) { action(); } };
        }

        protected sealed override Func<Task> TransformAsyncActionImpl(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties)
        {
            return async () =>
            {
                lock (LockGuard)
                {
                    while (LockGuard.IsLocked)
                        Monitor.Wait(LockGuard);
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
