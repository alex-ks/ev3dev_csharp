using Ev3Dev.CSharp.EvA.AttributeContracts;
using Ev3Dev.CSharp.EvA.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Generates a new property with type <see cref="bool"/> and name suffixed with `Changed`.
    /// This new property will be true if the original property value has changed since last check,
    /// and false ortherwise.
    /// </summary>
    /// <remarks>
    /// Note that if the original property was changed multiple times, the switch will detect only one
    /// change, or will not detect change at all if the original property was changed and changed back.
    /// Also there is a 'Cold start' problem - for the first check the switch will be `false` no matter
    /// many times it was changed before the first switch check.
    ///
    /// Also note that the switching value is changed inside the loop (e.g. by another action) the switch
    /// becomes sensitive to the actions call order.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SwitchAttribute : AbstractPropertyExtractor
    {
        public const string NameSuffix = "Changed";

        private static Func<bool> CreateSwitchGetter<T>(object target, PropertyInfo property)
        {
            var getter = DelegateGenerator.CreateGetter<T>(target, property);
            T cache = default(T);
            bool started = true;
            var comparer = EqualityComparer<T>.Default;

            return () =>
            {
                var value = getter();
                if (started)
                {
                    cache = value;
                    started = false;
                    return false;
                }
                var result = !comparer.Equals(value, cache);
                cache = value;
                return result;
            };
        }

        protected override (string, Delegate, Type) UnsafeExtractProperty(object target, PropertyInfo property)
        {
            if (property.GetCustomAttribute<SwitchAttribute>() == null)
                throw new ArgumentException("No switch attribute on property"); // todo: add message to resources

            var getter = typeof(SwitchAttribute).GetMethod(nameof(CreateSwitchGetter),
                                                           BindingFlags.Static | BindingFlags.NonPublic)
                                                .MakeGenericMethod(property.PropertyType);

            var switchGetter = getter.Invoke(this, new[] { target, property }) as Func<bool>;

            return (property.Name + NameSuffix, switchGetter, typeof(bool));
        }
    }
}
