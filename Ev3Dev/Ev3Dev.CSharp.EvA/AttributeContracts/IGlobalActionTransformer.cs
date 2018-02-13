using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IGlobalActionTransformer
    {
        IDictionary<string, Action> TransformActions(
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties,
            IReadOnlyDictionary<string, Action> actions,
            IReadOnlyDictionary<string, Func<Task>> asyncActions);
    }
}
