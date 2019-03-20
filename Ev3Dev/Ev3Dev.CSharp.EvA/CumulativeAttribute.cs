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
    /// For default model building (<see cref="LoopBuilder"/>) only makes sense for async methods.
    /// </summary>
    public class CumulativeAttribute : Attribute, IActionTransformer, ISynchronizedTransformer
    {
        public Action TransformAction(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties)
        {
            if (attributes.Count(attr => attr is ISynchronizedTransformer) > 1)
                throw new ArgumentException("Method must have only one ISynchronizedTransformer attribute");

            var lockGuard = new object();
            return () => { lock (lockGuard) { action(); } };
        }

        public Func<Task> TransformAsyncAction(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties)
        {
            if (attributes.Count(attr => attr is ISynchronizedTransformer) > 1)
                throw new ArgumentException("Method must have only one ISynchronizedTransformer attribute");

            var lockGuard = new object();
            var isLocked = false;

            return async () =>
            {
                if (!Monitor.TryEnter(lockGuard))
                {
                    await Task.Run(() =>
                    {
                        lock (lockGuard)
                        {
                            while (isLocked)
                                Monitor.Wait(lockGuard);
                            isLocked = true;
                        }
                    });
                }
                else if (isLocked)
                {
                    Monitor.Exit(lockGuard);
                    await Task.Run(() =>
                    {
                        lock (lockGuard)
                        {
                            while (isLocked)
                                Monitor.Wait(lockGuard);
                            isLocked = true;
                        }
                    });
                } 
                else
                {
                    isLocked = true;
                    Monitor.Exit(lockGuard);
                }

                try { await action(); }
                finally
                {
                    lock (lockGuard)
                    {
                        isLocked = false;
                        Monitor.Pulse(lockGuard);
                    }
                }
            };
        }
    }
}
