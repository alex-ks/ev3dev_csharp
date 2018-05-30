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
        public (Delegate, Type) ExtractProperty(object target, PropertyInfo property)
        {
            var (getter, type) = UnsafeExtractProperty(target, property);
            if (property.PropertyType == typeof(bool) && !(getter is Func<bool>))
                throw new InvalidOperationException("Boolean property getter must be Func<bool>"); // todo: add to resources
            if (property.PropertyType != typeof(bool) && !(getter is Func<object>))
                throw new InvalidOperationException("Non-boolean property getter must be Func<object>"); // todo: add to resources
            return (getter, type);
        }

        protected abstract (Delegate, Type) UnsafeExtractProperty(object target, PropertyInfo property);
    }
}
