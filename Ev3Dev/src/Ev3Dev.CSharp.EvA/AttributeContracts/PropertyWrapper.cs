using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public class PropertyWrapper
    {
        private readonly Delegate _getter;

        public Type Type { get; private set; }

        public Func<object> GenericGetter
        {
            get
            {
                if (Type == typeof(bool))
                    return () => (_getter as Func<bool>)();
                return _getter as Func<object>;
            }
        }

        public Func<bool> BooleanGetter => (Func<bool>)_getter;

        public PropertyWrapper(Type type, Delegate getter)
        {
            if (!TypeConsistent(type, getter))
                throw new ArgumentException("Property getter must be bool or object"); // todo: add to resources
            _getter = getter;
            Type = type;
        }

        private bool TypeConsistent(Type type, Delegate getter)
        {
            if (type == typeof(bool) && getter is Func<bool>)
                return true;
            if (type != typeof(bool) && getter is Func<object>)
                return true;
            return false;
        }
    }
}
