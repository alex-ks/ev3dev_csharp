using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EverythingIsNonCriticalAttribute : Attribute, ILoopTransformer
    {
        public LoopContents TransformLoop(LoopContents contents, object[] loopAttributes)
        {
            // todo: add message to resources
            if (loopAttributes.Where(attr => attr is EverythingIsNonCriticalAttribute).FirstOrDefault() != null)
                throw new InvalidOperationException("Ambiguous definition: EverythingIsNonCriticalAttribute is set too");

            var guarder = new NonCriticalAttribute();

            var guardedActions = new Dictionary<string, (Action, object[])>();
            foreach (var entry in contents.Actions)
                if (entry.Value.attributes.Where(attr => attr is CriticalAttribute || attr is NonCriticalAttribute)
                                          .FirstOrDefault() != null)
                    guardedActions[entry.Key] = entry.Value;
                else
                {
                    var guardedAction = guarder.TransformAction(entry.Key,
                                                                entry.Value.action,
                                                                entry.Value.attributes,
                                                                contents.Properties);
                    guardedActions[entry.Key] = (guardedAction, entry.Value.attributes);
                }


            var guardedAsyncs = new Dictionary<string, (Func<Task>, object[])>();
            foreach (var entry in contents.AsyncActions)
                if (entry.Value.attributes.Where(attr => attr is CriticalAttribute || attr is NonCriticalAttribute)
                                          .FirstOrDefault() != null)
                    guardedAsyncs[entry.Key] = entry.Value;
                else
                {
                    var guardedAction = guarder.TransformAsyncAction(entry.Key,
                                                                     entry.Value.action,
                                                                     entry.Value.attributes,
                                                                     contents.Properties);
                    guardedAsyncs[entry.Key] = (guardedAction, entry.Value.attributes);
                }

            return new LoopContents
            {
                Properties = contents.Properties,
                Actions = guardedActions,
                AsyncActions = guardedAsyncs
            };
        }
    }
}
