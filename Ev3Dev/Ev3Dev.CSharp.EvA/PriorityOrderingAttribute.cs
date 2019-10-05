using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PriorityOrderingAttribute : Attribute, IActionSorter
    {
        public int DefaultPriority { get; set; } = 10;

        public IEnumerable<(Action action, object[] attributes)> SortActions(
            IEnumerable<(Action action, object[] attributes)> actions)
        {
            return actions.OrderBy(
                pair => pair.attributes.Select(attr => attr as PriorityAttribute)
                                       .SingleOrDefault(attr => attr != null)?.Priority ?? DefaultPriority);
        }
    }
}
