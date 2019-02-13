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
        public string[] Methods { get; }

        /// <summary>
        /// Indicates whether to discard action or event handler if 
        /// one of the mutexed methods is running. True by default.
        /// </summary>
        public bool DiscardExcluded { get; set; } = true;

        public MutualExclusionAttribute( params string[] methods )
        {
            Methods = methods;
        }

        public LoopContents TransformLoop(LoopContents contents, object[] loopAttributes)
        {
            var exclusionGuard = new object();
            //bool locked = false;
            //string enteredName = null;
            int exclusionCounter = 0;
            int current = -1;

            var guardedActions = new Dictionary<string, (Action, object[])>();
            foreach (var entry in contents.Actions)
                guardedActions[entry.Key] = entry.Value;

            var guardedAsyncs = new Dictionary<string, (Func<Task>, object[])>();
            foreach (var entry in contents.AsyncActions)
                guardedAsyncs[entry.Key] = entry.Value;

            foreach (var methodName in Methods)
            {
                // Using indexing to allow multiple entrance for the same method
                // This delegates repeating entrances handling to non-reenterable attribute
                int exclusionIndex = exclusionCounter++;

                // These templates define how mutual exclusion is handled for method
                Func<Action, Action> GuardAction;
                Func<Func<Task>, Func<Task>> GuardAsyncAction;

                if (DiscardExcluded)
                {
                    GuardAction = action =>
                        () =>
                        {
                            lock (exclusionGuard)
                            {
                                if (current != -1)
                                { return; }
                                action();
                                Monitor.Pulse(exclusionGuard);
                            }
                        };
                    GuardAsyncAction = asyncAction =>
                        async () =>
                        {
                            lock (exclusionGuard)
                            {
                                if (current != -1 && current != exclusionIndex)
                                { return; }
                                current = exclusionIndex;
                            }
                            await asyncAction();
                            lock (exclusionGuard)
                            {
                                current = -1;
                                Monitor.Pulse(exclusionGuard);
                            }
                        };
                }
                else
                {
                    GuardAction = action =>
                        () =>
                        {
                            lock (exclusionGuard)
                            {
                                while (current != -1)
                                { Monitor.Wait(exclusionGuard); }
                                action();
                                Monitor.Pulse(exclusionGuard);
                            }
                        };
                    GuardAsyncAction = asyncAction =>
                        async () =>
                        {
                            lock (exclusionGuard)
                            {
                                while (current != -1 && current != exclusionIndex)
                                { Monitor.Wait(exclusionGuard); }
                                current = exclusionIndex;
                            }
                            await asyncAction();
                            lock (exclusionGuard)
                            {
                                current = -1;
                                Monitor.Pulse(exclusionGuard);
                            }
                        };
                }

                if (contents.Actions.ContainsKey(methodName))
                {
                    var (action, attributes) = contents.Actions[methodName];
                    if (!IsNonReenterable(attributes))
                        throw new InvalidOperationException(string.Format(Resources.NotNonReenterableMethod,
                                                                          methodName));
                    guardedActions[methodName] = 
                        (GuardAction(contents.Actions[methodName].action), contents.Actions[methodName].attributes);
                }
                else if (contents.AsyncActions.ContainsKey(methodName))
                {
                    var (asyncAction, attributes) = contents.AsyncActions[methodName];
                    if (!IsNonReenterable(attributes))
                        throw new InvalidOperationException(string.Format(Resources.NotNonReenterableMethod,
                                                                          methodName));
                    guardedAsyncs[methodName] =
                        (GuardAsyncAction(contents.AsyncActions[methodName].action), contents.Actions[methodName].attributes);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.MethodNotFound,
                                                                      methodName));
                }
            }

            return new LoopContents(contents.Properties, guardedActions, guardedAsyncs);
        }

        private bool IsNonReenterable(object[] attributes) =>
            attributes.Where(attr => attr is NonReenterableAttribute).FirstOrDefault() != null;
    }
}
