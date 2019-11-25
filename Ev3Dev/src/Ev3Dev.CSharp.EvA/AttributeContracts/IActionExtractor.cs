using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ev3Dev.CSharp.EvA.Reflection;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IActionExtractor
    {
        Action ExtractAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties);

        Func<Task> ExtractAsyncAction(
            object target,
            MethodInfo method,
            IReadOnlyDictionary<string, ICachingDelegate> properties);
    }
}
