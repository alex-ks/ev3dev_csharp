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
    /// Declares that method is not reenterable. If the method is called while another call is being executed,
    /// it will be pushed to a separate task to be launched after the previous call end. The loop will not await
    /// for the execution end.
    /// When using property forwarding, the properties will be took from the current iteration scope. In other words,
    /// the properties will be taken from scope where the previous call has been finished and the current call has
    /// been started.
    /// </summary>
    public class CumulativeAttribute : AbstractSynchronizedTransformer
    {
        protected sealed override Action TransformActionImpl(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            return () => { lock (LockGuard) { action(); } };
        }

        protected sealed override Func<Task> TransformAsyncActionImpl(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            return async () =>
            {
                if (!Monitor.TryEnter(LockGuard))
                {
                    await Task.Run(() =>
                    {
                        lock (LockGuard)
                        {
                            while (LockGuard.IsLocked)
                                Monitor.Wait(LockGuard);
                            LockGuard.IsLocked = true;
                        }
                    });
                }
                else if (LockGuard.IsLocked)
                {
                    Monitor.Exit(LockGuard);
                    await Task.Run(() =>
                    {
                        lock (LockGuard)
                        {
                            while (LockGuard.IsLocked)
                                Monitor.Wait(LockGuard);
                            LockGuard.IsLocked = true;
                        }
                    });
                }
                else
                {
                    LockGuard.IsLocked = true;
                    Monitor.Exit(LockGuard);
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
