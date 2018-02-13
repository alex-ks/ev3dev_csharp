using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IActionTransformer
    {
        Action TransformAction(
            string name, 
            Action action, 
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties);

        Action TransformAsyncAction(
            string name,
            Func<Task> action,
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties);
    }
}
