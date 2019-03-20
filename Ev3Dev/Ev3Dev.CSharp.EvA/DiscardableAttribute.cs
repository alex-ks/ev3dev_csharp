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
    /// will be discarded. For default model building (<see cref="LoopBuilder"/>) only makes sense for async methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DiscardableAttribute : Attribute, IActionTransformer, ISynchronizedTransformer
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

            return () =>
            {
                if (!Monitor.TryEnter(lockGuard))
                    return;
                try { action(); }
                finally { Monitor.Exit(lockGuard); }
            };
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
                lock (lockGuard)
                {
                    if (isLocked)
                        return;
                    isLocked = true;
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
