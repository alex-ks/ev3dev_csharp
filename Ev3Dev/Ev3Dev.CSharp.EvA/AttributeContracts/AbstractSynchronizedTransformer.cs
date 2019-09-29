using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    /// <summary>
    /// Provides an <see cref="LockGuard">LockGuard</see> object to synchronize on and performs general checks that
    /// make sense for attributes regulating simultaneous method calls.
    /// Also derivatives of this class can be used with <see cref="MutualExclusionAttribute"/>.
    /// </summary>
    public abstract class AbstractSynchronizedTransformer : Attribute, IActionTransformer
    {
        protected internal class ExclusionLockGuard
        {
            public bool IsLocked { get; set; } = false;
        }

        protected internal ExclusionLockGuard LockGuard { get; internal set; } = new ExclusionLockGuard();

        public Action TransformAction(string name, 
                                      Action action, 
                                      object[] attributes, 
                                      IReadOnlyDictionary<string, PropertyPack> properties)
        {
            if (attributes.Count(attr => attr is AbstractSynchronizedTransformer) > 1)
                throw new ArgumentException("Method must have only one synchronization attribute");
            return TransformActionImpl(name, action, attributes, properties);
        }

        public Func<Task> TransformAsyncAction(string name, 
                                               Func<Task> action, 
                                               object[] attributes, 
                                               IReadOnlyDictionary<string, PropertyPack> properties)
        {
            if (attributes.Count(attr => attr is AbstractSynchronizedTransformer) > 1)
                throw new ArgumentException("Method must have only one synchronization attribute");
            return TransformAsyncActionImpl(name, action, attributes, properties);
        }

        protected abstract Action TransformActionImpl(string name,
                                                      Action action,
                                                      object[] attributes,
                                                      IReadOnlyDictionary<string, PropertyPack> properties);

        protected abstract Func<Task> TransformAsyncActionImpl(string name,
                                                               Func<Task> action,
                                                               object[] attributes,
                                                               IReadOnlyDictionary<string, PropertyPack> properties);
    }
}
