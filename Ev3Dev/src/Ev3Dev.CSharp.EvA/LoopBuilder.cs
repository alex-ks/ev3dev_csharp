﻿using Ev3Dev.CSharp.EvA.AttributeContracts;
using Ev3Dev.CSharp.EvA.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    public static class LoopBuilder
    {
        /// <summary>
        /// Creates an EventLoop from a model according to `EvA.AttributeContracts`.
        /// </summary>
        /// <param name="model">An object to be parsed.</param>
        /// <param name="loadPropertiesLazily">
        /// Specifies whether the properties should be cached actively at the beginning of a loop iteration,
        /// or lazily on a first getter call. In any case, all the properties will be cached until the next iteration
        /// to guarantee their persistance during a single iteration.
        /// </param>
        /// <param name="allowEndless">Specifies whether the loop can be built without any shutdown events.</param>
        public static EventLoop BuildLoop(
            this object model,
            bool loadPropertiesLazily = true,
            bool allowEndless = false)
        {
            var properties = ExtractProperties(model);

            // special case - shutdown events
            var shutdownEvents = ExtractShutdownEvents(model, properties);

            if (!allowEndless && shutdownEvents.Count == 0)
                throw new InvalidOperationException(Resources.NoShutdownEvent);

            // all actions must have different names (no overload)
            var actions = ExtractContents(model, properties)
                                    .TransformActions()
                                    .TransformLoop(model)
                                    .OrderActions()
                                    .ToList();

            Action fillCache = () =>
            {
                foreach (var pair in properties)
                    pair.Value.PopulateCache();
            };

            Action clearCache = () =>
            {
                foreach (var pair in properties)
                    pair.Value.ClearCache();
            };

            var loop = new EventLoop(fillCache, clearCache) { LoadPropertiesLazily = loadPropertiesLazily };

            for (int i = 0; i < actions.Count; ++i)
                loop.RegisterAction(actions[i], i);

            foreach (var shutdownEvent in shutdownEvents)
                loop.RegisterShutdownEvent(shutdownEvent);

            return loop;
        }

        private static Dictionary<string, ICachingDelegate> ExtractProperties(
            object model)
        {
            // All properties names must be unique.
            var getters = new Dictionary<string, ICachingDelegate>();

            foreach (var prop in model.GetType().GetProperties())
            {
                // plain getters
                getters.Add(prop.Name, CachingDelegateCreator.CreateCachingZeroArgDelegate(
                                                DelegateGenerator.CreateRawGetter(model, prop)));

                // custom getters
                foreach (var extractor in prop.GetCustomAttributes(inherit: true)
                                              .Where(attr => attr is AbstractPropertyExtractor)
                                              .Select(attr => attr as AbstractPropertyExtractor))
                {
                    var (name, customGetter, t) = extractor.ExtractProperty(model, prop);
                    if (getters.ContainsKey(name))
                        throw new InvalidOperationException(
                            string.Format("Name {0} of property generated by {1} is already used",
                                          name,
                                          extractor.GetType().Name));
                    getters.Add(name, CachingDelegateCreator.CreateCachingZeroArgDelegate(customGetter));
                }
            }

            return getters;
        }

        private static List<Func<bool>> ExtractShutdownEvents(object model,
                                                              IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            var shutdownEvents = from prop in model.GetType().GetProperties()
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>()
                                 where attribute != null
                                 select prop.Name;

            var customShutdownEvents = from attr in model.GetType().GetCustomAttributes<CustomShutdownEventAttribute>()
                                       select attr.Name;

            foreach (var customProp in customShutdownEvents)
            {
                if (!properties.ContainsKey(customProp))
                    throw new InvalidOperationException(  // todo: add to resources.
                            "CustomShutdownEvent is declared with unexisting property as argument.");
            }

            var shutdownEventsGetters = shutdownEvents.Concat(customShutdownEvents)
                                                      .Where(prop => properties[prop].Delegate
                                                                                     .Method
                                                                                     .ReturnType == typeof(bool))
                                                      .Select(prop => properties[prop].Delegate as Func<bool>)
                                                      .ToList();

            return shutdownEventsGetters;
        }

        private static ActionContents ExtractContents(
            object model,
            IReadOnlyDictionary<string, ICachingDelegate> properties)
        {
            var actions = new Dictionary<string, (Action action, object[] attributes)>();
            var asyncActions = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            var names = new HashSet<string>();

            foreach (var method in model.GetType().GetMethods())
            {
                // Reflection attributes are recreated on each GetCustomAttributes call, so we need to be sure we call
                // it only once.
                var attributes = method.GetCustomAttributes(true);
                IActionExtractor extractor;

                try
                {
                    extractor = attributes.Select(attr => attr as IActionExtractor)
                                          .Where(attr => attr != null)
                                          .SingleOrDefault();
                }
                catch (InvalidOperationException)
                {
                    // todo: add to resources
                    throw new InvalidOperationException("Method should have only one action extractor");
                }

                if (extractor == null)
                    continue;

                if (method.ReturnType == typeof(void))
                {
                    var action = extractor.ExtractAction(model, method, properties);

                    if (names.Contains(method.Name))  // todo: add to resources
                        throw new InvalidOperationException("All actions must have different names");

                    actions.Add(method.Name, (action, attributes));
                }
                else if (method.ReturnType == typeof(Task))
                {
                    var action = extractor.ExtractAsyncAction(model, method, properties);

                    if (names.Contains(method.Name))  // todo: add to resources
                        throw new InvalidOperationException("All actions must have different names");

                    asyncActions.Add(method.Name, (action, attributes));
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.InvalidAsyncAction,
                                                                      method.Name));
                }
                names.Add(method.Name);
            }

            return new ActionContents(properties, actions, asyncActions);
        }

        private static ActionContents TransformActions(this ActionContents contents)
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

            return new ActionContents(contents.Properties, transformedActions, transformedAsyncs);
        }

        private static LoopContents TransformLoop(this ActionContents contents, object model)
        {
            var transformedActions = new Dictionary<string, (Action action, object[] attributes)>();
            var transformedAsyncs = new Dictionary<string, (Func<Task> action, object[] attributes)>();

            var attributes = model.GetType().GetCustomAttributes(true);

            var transformers = attributes.Select(attr => attr as ILoopTransformer)
                                         .Where(attr => attr != null);

            foreach (var transformer in transformers)
            {
                contents = transformer.TransformLoop(contents, attributes);
            }

            return contents.ToLoopContents(attributes);
        }

        private static IEnumerable<Action> OrderActions(this LoopContents contents)
        {
            var sorters = contents.ModelAttributes.Select(attr => attr as IActionSorter)
                                                  .Where(attr => attr != null);
            return sorters.Aggregate(contents.Actions, (acts, sorter) => sorter.SortActions(acts))
                          .Select(t => t.action);
        }
    }
}
