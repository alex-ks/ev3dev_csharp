using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Declares that any exception in the selected method will stop loop execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CriticalAttribute : Attribute, IActionTransformer
    {
        public bool LogExceptions { get; set; } = true;

        public Action TransformAction(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            // todo: add message to resources
            if (attributes.FirstOrDefault(attr => attr is NonCriticalAttribute) != null)
                throw new ArgumentException("Method must have only one Critical or NonCritical attribute");

            if (LogExceptions)
                return () =>
                {
                    try { action(); }
                    catch (LoopInterruptedException e) { Console.Error.WriteLine(e); throw; }
                    catch (Exception e) { Console.Error.WriteLine(e); throw new LoopInterruptedException(e); }
                };

            return () =>
            {
                try { action(); }
                catch (LoopInterruptedException) { throw; }
                catch (Exception e) { throw new LoopInterruptedException(e); }
            };
        }

        public Func<Task> TransformAsyncAction(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyWrapper> properties)
        {
            // todo: add message to resources
            if (attributes.FirstOrDefault(attr => attr is NonCriticalAttribute) != null)
                throw new ArgumentException("Method must have only one Critical or NonCritical attribute");

            if (LogExceptions)
                return async () =>
                {
                    try { await action(); }
                    catch (LoopInterruptedException e) { Console.Error.WriteLine(e); throw; }
                    catch (Exception e) { Console.Error.WriteLine(e); throw new LoopInterruptedException(e); }
                };

            return async () =>
            {
                try { await action(); }
                catch (LoopInterruptedException) { throw; }
                catch (Exception e) { throw new LoopInterruptedException(e); }
            };
        }
    }
}
