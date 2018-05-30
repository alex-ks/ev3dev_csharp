using System;
using System.Collections.Generic;
using System.Linq;

namespace Ev3Dev.CSharp.EvA
{
    public class PropertyStorage
    {
        private readonly IReadOnlyDictionary<Type, Delegate> _getters;

        public Func<object> this[Type t]
        {
            get
            {
                if (t == typeof(bool))
                    return () => (_getters[t] as Func<bool>)();
                return _getters[t] as Func<object>;
            }
        }

        public Func<bool> Boolean => _getters[typeof(bool)] as Func<bool>;

        public bool ContainsKey(Type t) => _getters.ContainsKey(t);

        private bool PairConsistent(KeyValuePair<Type, Delegate> pair)
        {
            if (pair.Key == typeof(bool) && pair.Value is Func<bool>)
                return true;
            if (pair.Key != typeof(bool) && pair.Value is Func<object>)
                return true;
            return false;
        }

        public PropertyStorage(IReadOnlyDictionary<Type, Delegate> getters)
        {
            if (!(getters as IEnumerable<KeyValuePair<Type, Delegate>>).All(PairConsistent))
                throw new ArgumentException("Property getter must be bool or object"); // todo: add to resources

            _getters = getters;
        }
    }
}
