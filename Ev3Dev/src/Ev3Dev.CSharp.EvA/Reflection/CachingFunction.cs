using System;

namespace Ev3Dev.CSharp.EvA.Reflection
{
    public class CachingFunction<T> : ICachingDelegate
    {
        private bool _isCachePopulated;
        private T _cache;
        private Func<T> _originalFunction;

        public Func<T> Function { get; private set; }

        public Delegate Delegate => Function;

        public CachingFunction(Func<T> function)
        {
            _originalFunction = function;
            Function = () => {
                if (!_isCachePopulated)
                {
                    _cache = _originalFunction();
                    _isCachePopulated = true;
                }
                return _cache;
            };
        }

        public void ClearCache()
        {
            _isCachePopulated = false;
            _cache = default(T);
        }

        public void PopulateCache()
        {
            _cache = _originalFunction();
            _isCachePopulated = true;
        }
    }
}