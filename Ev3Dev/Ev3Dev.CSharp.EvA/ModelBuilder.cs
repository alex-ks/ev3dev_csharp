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
            var loopContents = ExtractContents(model, properties).TransformActions();

            throw new NotImplementedException();
        }

        private static Dictionary<string, PropertyPack> ExtractProperties(object model)
        {
            var getters = new Dictionary<string, PropertyPack>();

            foreach (var prop in model.GetType().GetProperties())
            {
                // expecting no same-named properties
                var dict = new Dictionary<Type, Delegate>();

                // plain getters
                if (prop.PropertyType == typeof(bool))
                    dict.Add(prop.PropertyType, DelegateGenerator.CreateGetter<bool>(model, prop));
                else
                    dict.Add(prop.PropertyType, DelegateGenerator.CreateGetter(model, prop));

                // custom getters
                foreach (var extractor in prop.GetCustomAttributes(inherit: true)
                                              .Where(attr => attr is AbstractPropertyExtractor)
                                              .Select(attr => attr as AbstractPropertyExtractor))
                {
                    var (customGetter, t) = extractor.ExtractProperty(model, prop);
                    dict.Add(t, customGetter);
                }

                getters.Add(prop.Name, new PropertyPack(dict));
            }

            return getters;
        }

        private static List<Func<bool>> ExtractShutdownEvents(object model,
                                                              IReadOnlyDictionary<string, PropertyPack> properties)
        {
            var shutdownEvents = from prop in model.GetType().GetProperties()
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>()
                                 where attribute != null
                                 select prop;

            var shutdownEventsGetters = shutdownEvents.Select(prop => properties[prop.Name].Boolean).ToList();

            return shutdownEventsGetters;
        }

        private static LoopContents ExtractContents(object model, IReadOnlyDictionary<string, PropertyPack> properties)
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

            return new LoopContents
            {
                Properties = properties,
                Actions = actions,
                AsyncActions = asyncActions
            };
        }

        private static LoopContents TransformActions(this LoopContents contents)
        {
            var transformedActions = new Dictionary<string, (Action action, object[] attributes)>();
            var transformedAsyncs = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            foreach (var pair in contents.Actions)
            {
                var transformers = pair.Value.attributes.Select(attr => attr as IActionTransformer)
                                                        .Where(attr => attr != null);

                var transformed = pair.Value.action;

                foreach (var transformer in transformers)
                    transformed = transformer.TransformAction(pair.Key, 
                                                              transformed, 
                                                              pair.Value.attributes, 
                                                              contents.Properties);

                transformedActions.Add(pair.Key, (transformed, pair.Value.attributes));
            }

            foreach (var pair in contents.AsyncActions)
            {
                var transformers = pair.Value.attributes.Select(attr => attr as IActionTransformer)
                                                        .Where(attr => attr != null);

                var transformed = pair.Value.action;

                foreach (var transformer in transformers)
                    transformed = transformer.TransformAsyncAction(pair.Key, 
                                                                   transformed, 
                                                                   pair.Value.attributes, 
                                                                   contents.Properties);

                transformedAsyncs.Add(pair.Key, (transformed, pair.Value.attributes));
            }

            return new LoopContents
            {
                Actions = transformedActions,
                AsyncActions = transformedAsyncs,
                Properties = contents.Properties
            };
        }

        private static LoopContents TransformLoop(object model, LoopContents contents)
        {
            var transformedActions = new Dictionary<string, (Action action, object[] attributes)>();
            var transformedAsyncs = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            var transformers = model.GetType()
                                    .GetCustomAttributes()
                                    .Select(attr => attr as ILoopTransformer)
                                    .Where(attr => attr != null);

            foreach (var transformer in transformers)
            {
                contents = transformer.TransformLoop(contents);
            }

            return contents;
        }
    }
}
