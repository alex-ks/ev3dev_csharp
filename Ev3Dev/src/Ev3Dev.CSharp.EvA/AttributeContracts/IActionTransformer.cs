using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ev3Dev.CSharp.EvA.Reflection;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IActionTransformer
    {
        Action TransformAction(
            string name,
            Action action,
            object[] attributes,
            IReadOnlyDictionary<string, ICachingDelegate> properties);

        Func<Task> TransformAsyncAction(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, ICachingDelegate> properties);
    }
}
