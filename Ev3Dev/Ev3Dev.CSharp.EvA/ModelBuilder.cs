using Ev3Dev.CSharp.EvA.AttributeContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    public static class ModelBuilder
    {
        public static EventLoop Build(
            this EventLoop loop, 
            object model,
            bool treatMethodsAsCritical = true,
            bool logExceptionsByDefault = true,
            bool allowEndless = false)
        {   
            var properties = ExtractProperties(model);

            // special case - shutdown events
            var shutdownEvents = ExtractShutdownEvents(model, properties);

            if (!allowEndless && shutdownEvents.Count == 0)
                throw new InvalidOperationException(Resources.NoShutdownEvent);

            // all actions must have different names (no overload)

            throw new NotImplementedException();
        }

        private static Dictionary<string, Dictionary<Type, Func<object>>> ExtractProperties(object model)
        {
            Dictionary<string, Dictionary<Type, Func<object>>> getters =
                new Dictionary<string, Dictionary<Type, Func<object>>>();

            foreach (var prop in model.GetType().GetProperties())
            {
                // expecting no same-named properties
                getters.Add(prop.Name, new Dictionary<Type, Func<object>>());

                // plain getters
                var getter = DelegateGenerator.CreateGetter(model, prop);
                getters[prop.Name].Add(prop.PropertyType, getter);

                // custom getters
                foreach (var extractor in prop.GetCustomAttributes(inherit: true)
                                              .Where(attr => attr is IPropertyExtractor)
                                              .Select(attr => attr as IPropertyExtractor))
                {
                    var (customGetter, t) = extractor.ExtractProperty(model, prop);
                    getters[prop.Name].Add(t, customGetter);
                }
            }

            return getters;
        }

        private static List<Func<bool>> ExtractShutdownEvents(
            object model,
            Dictionary<string, Dictionary<Type, Func<object>>> properties)
        {
            var shutdownEvents = from prop in model.GetType().GetProperties()
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>()
                                 where attribute != null
                                 select prop;

            var shutdownEventsGetters = new List<Func<bool>>();

            foreach (var prop in shutdownEvents)
            {
                Func<bool> shutdownEvent = null;
                
                // small optimization - if property is exactly bool, get it instead of casting
                if (prop.PropertyType == typeof(bool))
                    shutdownEvent = DelegateGenerator.CreateGetter<bool>(model, prop);
                else if (properties[prop.Name].ContainsKey(typeof(bool)))
                {
                    var getter = properties[prop.Name][typeof(bool)];
                    shutdownEvent = () => true.Equals(getter());
                }

                if (shutdownEvent == null)
                    throw new InvalidOperationException(Resources.InvalidShutdownEvent);

                shutdownEventsGetters.Add(shutdownEvent);
            }

            return shutdownEventsGetters;
        }
    }
}
