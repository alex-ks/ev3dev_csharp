using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MutualExclusionAttribute : Attribute, ILoopTransformer
    {
        internal ISet<string> MethodsToGuard { get; }

        public MutualExclusionAttribute(params string[] methods)
        {
            MethodsToGuard = new SortedSet<string>(methods);
        }

        public ActionContents TransformLoop(ActionContents contents, object[] loopAttributes)
        {
            // Preconditions check.
            foreach (var attr in loopAttributes)
            {
                var anotherMutex = attr as MutualExclusionAttribute;
                // This requires GetCustomAttributes to be called only once.
                if (anotherMutex == null || anotherMutex == this)
                    continue;
                if (MethodsToGuard.Overlaps(anotherMutex.MethodsToGuard))
                    throw new ArgumentException("Every action may be used only in one mutual exclusion.");
            }

            foreach (var name in MethodsToGuard)
            {
                if (!contents.Actions.ContainsKey(name) && !contents.AsyncActions.ContainsKey(name))
                    throw new ArgumentException(Resources.MethodNotFound);
            }

            // Change LockGuard of every listed AbstractSynchronizedTransformer to single synchronization point.
            var exclusionGuard = new AbstractSynchronizedTransformer.ExclusionLockGuard();

            var guardedActions = new Dictionary<string, (Action, object[])>();
            foreach (var entry in contents.Actions)
            {
                if (MethodsToGuard.Contains(entry.Key))
                {
                    var synchronizer = FindSynchronizedTransformer(entry.Value.attributes);
                    if (synchronizer == null)
                        throw new InvalidOperationException(string.Format(Resources.NotNonReenterableMethod, entry.Key));
                    synchronizer.LockGuard = exclusionGuard;
                }
                guardedActions[entry.Key] = entry.Value;
            }

            var guardedAsyncs = new Dictionary<string, (Func<Task>, object[])>();
            foreach (var entry in contents.AsyncActions)
            {
                if (MethodsToGuard.Contains(entry.Key))
                {
                    var synchronizer = FindSynchronizedTransformer(entry.Value.attributes);
                    if (synchronizer == null)
                        throw new InvalidOperationException(Resources.NotNonReenterableMethod);
                    synchronizer.LockGuard = exclusionGuard;
                }
                guardedAsyncs[entry.Key] = entry.Value;
            }

            return new ActionContents(contents.Properties, guardedActions, guardedAsyncs);
        }


        private AbstractSynchronizedTransformer FindSynchronizedTransformer(object[] attributes) =>
            attributes.Select(attr => attr as AbstractSynchronizedTransformer).SingleOrDefault(attr => attr != null);
    }
}
