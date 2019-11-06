using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.AttributeContracts
{
    public abstract class AbstractPropertyExtractor : Attribute
    {
        public (string, Delegate, Type) ExtractProperty(object target, PropertyInfo property)
        {
            var (name, getter, type) = UnsafeExtractProperty(target, property);
            if (type == typeof(bool) && !(getter is Func<bool>))
                throw new InvalidOperationException("Boolean property getter must be Func<bool>"); // todo: add to resources
            if (type != typeof(bool) && !(getter is Func<object>))
                throw new InvalidOperationException("Non-boolean property getter must be Func<object>"); // todo: add to resources
            return (name, getter, type);
        }

        protected abstract (string, Delegate, Type) UnsafeExtractProperty(object target, PropertyInfo property);
    }
}
