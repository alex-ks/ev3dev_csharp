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
            IReadOnlyDictionary<string, PropertyStorage> properties,
            IReadOnlyDictionary<string, (Action action, object[] attributes)> actions,
            IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> asyncActions);
    }
}
