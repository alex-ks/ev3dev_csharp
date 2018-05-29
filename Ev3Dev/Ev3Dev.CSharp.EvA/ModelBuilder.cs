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
            var (actions, asyncActions) = ExtractActions(model, properties);

            var (transformedActions, transformedAsyncs) = TransformActions(actions, asyncActions, properties);

            throw new NotImplementedException();
        }

        private static Dictionary<string, IReadOnlyDictionary<Type, Func<object>>> ExtractProperties(object model)
        {
            Dictionary<string, IReadOnlyDictionary<Type, Func<object>>> getters =
                new Dictionary<string, IReadOnlyDictionary<Type, Func<object>>>();

            foreach (var prop in model.GetType().GetProperties())
            {
                // expecting no same-named properties
                var dict = new Dictionary<Type, Func<object>>();
                getters.Add(prop.Name, dict);

                // plain getters
                var getter = DelegateGenerator.CreateGetter(model, prop);
                dict.Add(prop.PropertyType, getter);

                // custom getters
                foreach (var extractor in prop.GetCustomAttributes(inherit: true)
                                              .Where(attr => attr is IPropertyExtractor)
                                              .Select(attr => attr as IPropertyExtractor))
                {
                    var (customGetter, t) = extractor.ExtractProperty(model, prop);
                    dict.Add(t, customGetter);
                }
            }

            return getters;
        }

        private static List<Func<bool>> ExtractShutdownEvents(
            object model,
            IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
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

        private static (IReadOnlyDictionary<string, (Action action, object[] attributes)>, 
                        IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)>)
            ExtractActions(object model, IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
        {
            var actions = new Dictionary<string, (Action action, object[] attributes)>();
            var asyncActions = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            foreach (var method in model.GetType().GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);
                var extractor = attributes.Select(attr => attr as IActionExtractor).Where(attr => attr != null).Single();

                if (method.ReturnType == typeof(void))
                {
                    var action = extractor.ExtractAction(model, method, properties);
                    actions.Add(method.Name, (action, attributes));
                }
                else if (method.ReturnType == typeof(Task))
                {
                    var action = extractor.ExtractAsyncAction(model, method, properties);
                    asyncActions.Add(method.Name, (action, attributes));
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.InvalidAsyncAction,
                                                                      method.Name));
                }
            }

            return (actions, asyncActions);
        }

        private static (IReadOnlyDictionary<string, (Action action, object[] attributes)>,
                        IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)>)
            TransformActions( 
                IReadOnlyDictionary<string, (Action action, object[] attributes)> syncActions,
                IReadOnlyDictionary<string, (Func<Task> action, object[] attributes)> asyncActions,
                IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<object>>> properties)
        {
            var transformedActions = new Dictionary<string, (Action action, object[] attributes)>();
            var transformedAsyncs = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            foreach (var pair in syncActions)
            {
                var transformers = pair.Value.attributes.Select(attr => attr as IActionTransformer)
                                                        .Where(attr => attr != null);

                var transformed = pair.Value.action;

                foreach (var transformer in transformers)
                    transformed = transformer.TransformAction(pair.Key, transformed, pair.Value.attributes, properties);

                transformedActions.Add(pair.Key, (transformed, pair.Value.attributes));
            }

            foreach (var pair in asyncActions)
            {
                var transformers = pair.Value.attributes.Select(attr => attr as IActionTransformer)
                                                        .Where(attr => attr != null);

                var transformed = pair.Value.action;

                foreach (var transformer in transformers)
                    transformed = transformer.TransformAsyncAction(pair.Key, transformed, pair.Value.attributes, properties);

                transformedAsyncs.Add(pair.Key, (transformed, pair.Value.attributes));
            }

            return (transformedActions, transformedAsyncs);
        }
    }
}
