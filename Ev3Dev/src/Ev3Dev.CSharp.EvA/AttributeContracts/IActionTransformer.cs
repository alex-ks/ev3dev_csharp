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
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties);

        Func<Task> TransformAsyncAction(
            string name,
            Func<Task> action,
            object[] attributes,
            IReadOnlyDictionary<string, PropertyPack> properties);
    }
}
