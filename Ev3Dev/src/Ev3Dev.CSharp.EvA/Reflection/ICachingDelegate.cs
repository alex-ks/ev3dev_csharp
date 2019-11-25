using System;

namespace Ev3Dev.CSharp.EvA.Reflection
{
    public interface ICachingDelegate
    {
        Delegate Delegate { get; }
        void PopulateCache();
        void ClearCache();
    }
}