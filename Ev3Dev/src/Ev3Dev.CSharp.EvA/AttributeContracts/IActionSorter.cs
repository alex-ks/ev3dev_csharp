using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IActionSorter
    {
        IEnumerable<(Action action, object[] attributes)> SortActions(
            IEnumerable<(Action action, object[] attributes)> actions);
    }
}
