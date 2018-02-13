using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public interface IPropertyExtractor
    {
        (Func<object>, Type) ExtractProperty(object target, PropertyInfo property);
    }
}
