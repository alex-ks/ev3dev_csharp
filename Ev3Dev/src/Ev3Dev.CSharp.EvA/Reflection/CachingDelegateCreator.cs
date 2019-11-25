using System;
using System.Reflection;

namespace Ev3Dev.CSharp.EvA.Reflection
{
    public static class CachingDelegateCreator
    {
        private static ICachingDelegate CreateCachingZeroArgDelegateGeneric<T>(Delegate func)
        {
            var getter = func as Func<T>;
            if (getter == null)
                throw new ArgumentException("Type mismatch");
            return new CachingFunction<T>(getter);
        }

        public static ICachingDelegate CreateCachingZeroArgDelegate(Delegate func)
        {
            var creator = typeof(CachingDelegateCreator)
                            .GetMethod(nameof(CreateCachingZeroArgDelegateGeneric),
                                       BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(func.Method.ReturnType);
            return creator.Invoke(null, new[] { func }) as ICachingDelegate;
        }
    }
}