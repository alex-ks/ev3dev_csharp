using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    /// <summary>
    /// Though not presenting any contracts, this interface signalizes that transformed action is guarded
    /// against simultaneous calls and thus can be used in synchronization constructs of higher level, such as
    /// <see cref="MutualExclusionAttribute"/>.
    /// </summary>
    public interface ISynchronizedTransformer { }
}
