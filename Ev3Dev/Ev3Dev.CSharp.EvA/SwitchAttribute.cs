using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Indicates that marked property can be used as <see cref="bool"/> value.
    /// In this case, property value will be true if property value has changed since last check,
    /// and false ortherwise.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SwitchAttribute : AbstractPropertyExtractor
    {
        protected override (Delegate, Type) UnsafeExtractProperty(object target, PropertyInfo property)
        {
            if (property.GetCustomAttribute<SwitchAttribute>() == null)
                throw new ArgumentException("No switch attribute on property"); // todo: add message to resources

            if (property.PropertyType == typeof(bool))
                throw new InvalidOperationException(string.Format(Resources.AmbiguousPropertyUse,
                                                                  property.Name));

            var getter = DelegateGenerator.CreateGetter(target, property);
            object cache = null;
            bool started = true;

            Func<bool> switchGetter = () =>
            {
                var obj = getter();
                if (started)
                {
                    started = false;
                    return false;
                }
                var result = !Equals(obj, cache);
                cache = obj;
                return result;
            };

            return (switchGetter, typeof(bool));
        }
    }
}
