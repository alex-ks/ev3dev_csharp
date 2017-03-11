using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Provides extension for <see cref="EventLoop"/> to allow registering attribute-marked objects.
    /// </summary>
    public static class ModelParser
    {
        private struct EventHandler
        {
            public Func<bool> Trigger { get; set; }
            public Action Handler { get; set; }
        }

        /// <summary>
        /// Looks for attribute-marked methods and properties and adds them to event loop.
        /// </summary>
        /// <param name="loop">A loop that will handle model methods.</param>
        /// <param name="model">Object which methods will be used to generate loop actions and events.</param>
        /// <param name="treatMethodsAsCritical">
        /// If true, all methods without <see cref="CriticalAttribute"/> or <see cref="NonCriticalAttribute"/>
        /// will be treated as critical (otherwise - non-critical).
        /// </param>
        /// <param name="allowEndless">
        /// If equals to false, parser will check existense of shutdown events and will throw exception if there is noone.
        /// </param>
        public static void RegisterModel( this EventLoop loop,
                                          object model,
                                          bool treatMethodsAsCritical = true,
                                          bool logExceptionsByDefault = true,
                                          bool allowEndless = false )
        {
            var type = model.GetType( );

            #region Switch generation

            // create events from switch properties

            var switches = new Dictionary<string, Func<bool>>( );

            var switchCandidates = from prop in type.GetProperties( )
                                   let attribute = prop.GetCustomAttribute<SwitchAttribute>( )
                                   where attribute != null
                                   select prop;

            foreach ( var prop in switchCandidates )
            {
                if ( prop.PropertyType == typeof( bool ) )
                {
                    throw new InvalidOperationException( string.Format( Resources.AmbiguousPropertyUse,
                                                                        prop.Name ) );
                }

                var getter = DelegateGenerator.CreateGetter( model, prop );
                object cache = null;
                bool started = true;
                Func<bool> switchGetter = ( ) =>
                {
                    var obj = getter( );
                    if ( started )
                    {
                        started = false;
                        return false;
                    }
                    var result = !Equals( obj, cache );
                    cache = obj;
                    return result;
                };
                switches.Add( prop.Name, switchGetter );
            }

            #endregion

            #region Shutdown events generation

            var shutdownEvents = from prop in type.GetProperties( )
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>( )
                                 where attribute != null
                                 select prop;

            var shutdownEventsToAdd = new List<Func<bool>>( );

            // parse events that will cause loop shutdown
            foreach ( var prop in shutdownEvents )
            {
                Func<bool> shutdownEvent = null;
                if ( prop.PropertyType == typeof( bool ) )
                { shutdownEvent = DelegateGenerator.CreateGetter<bool>( model, prop ); }
                if ( switches.ContainsKey( prop.Name ) )
                { shutdownEvent = switches[prop.Name]; }

                if ( shutdownEvent == null )
                { throw new InvalidOperationException( Resources.InvalidShutdownEvent ); }

                shutdownEventsToAdd.Add( shutdownEvent );
            }

            if ( !allowEndless && shutdownEventsToAdd.Count == 0 )
            {
                throw new InvalidOperationException( Resources.NoShutdownEvent );
            }

            #endregion

            #region Actions generation

            bool hasCriticalActions = false;

            // parse actions that will be performed on each iteration
            var actions = from method in type.GetMethods( )
                          let attribute = method.GetCustomAttribute<ActionAttribute>( )
                          let priorityAttribute = method.GetCustomAttribute<PriorityAttribute>( )
                          where attribute != null
                          select new
                          {
                              Method = method,
                              Priority = priorityAttribute?.Priority ?? int.MaxValue
                          };

            var actionsToAdd = new List<OrderedAction>( );
            var asyncsToAdd = new List<OrderedFunc<Task>>( );

            // to keep actions that potentially need mutual exclusion
            var nonReenterableMethods = new Dictionary<string, OrderedAction>( );
            var nonReenterableAsyncs = new Dictionary<string, OrderedFunc<Task>>( );

            foreach ( var pair in actions )
            {
                bool reenterable;
                var method = pair.Method;

                if ( method.GetCustomAttribute<CriticalAttribute>( ) != null )
                { hasCriticalActions = true; }

                if ( !DelegateGenerator.IsAsync( method ) )
                {
                    Action performAction = GetMethodAction( model, 
                                                            method, 
                                                            out reenterable );
                    performAction = AddExceptionHandling( performAction,
                                                          method,
                                                          model,
                                                          loop,
                                                          treatMethodsAsCritical,
                                                          logExceptionsByDefault );
                    if ( reenterable )
                    { actionsToAdd.Add( new OrderedAction( performAction, pair.Priority ) ); }
                    else
                    { nonReenterableMethods.Add( method.Name, new OrderedAction( performAction, pair.Priority ) ); }
                }
                else
                {
                    Func<Task> performAsync = GetAsyncMethodAction( model, 
                                                                    method, 
                                                                    out reenterable );
                    performAsync = AddAsyncExceptionHandling( performAsync,
                                                              method,
                                                              model,
                                                              loop,
                                                              treatMethodsAsCritical,
                                                              logExceptionsByDefault );
                    if ( reenterable )
                    { asyncsToAdd.Add( new OrderedFunc<Task>( performAsync, pair.Priority ) ); }
                    else
                    { nonReenterableAsyncs.Add( method.Name, new OrderedFunc<Task>( performAsync, pair.Priority ) ); }
                }
            }

            #endregion

            #region Events and handlers generation

            // parse event handlers and their triggers
            var eventHandlersToAdd = new List<Tuple<Func<bool>, OrderedAction>>( );
            var eventAsyncsToAdd = new List<Tuple<Func<bool>, OrderedFunc<Task>>>( );

            // to keep handlers that potentially need mutual exclusion
            var nonReenterableEventHandlers = new Dictionary<string, Tuple<Func<bool>, OrderedAction>>( );
            var nonReenterableEventAsyncs = new Dictionary<string, Tuple<Func<bool>, OrderedFunc<Task>>>( );

            var eventHandlers = from method in type.GetMethods( )
                                let attribute = method.GetCustomAttribute<EventHandlerAttribute>( )
                                let priorityAttribute = method.GetCustomAttribute<PriorityAttribute>( )
                                where attribute != null
                                select new
                                {
                                    Method = method,
                                    Attribute = attribute,
                                    Priority = priorityAttribute?.Priority ?? int.MaxValue
                                };
            
            foreach ( var eventHandler in eventHandlers )
            {
                var method = eventHandler.Method;
                var triggers = new List<Func<bool>>( );

                if ( method.GetCustomAttribute<CriticalAttribute>( ) != null )
                { hasCriticalActions = true; }

                if ( eventHandler.Attribute.Triggers.Length == 0 )
                {
                    throw new InvalidOperationException( string.Format( Resources.InvalidEventTriggerCount,
                                                                        method.Name ) );
                }

                // get trigger flags
                foreach ( var trigger in eventHandler.Attribute.Triggers )
                {
                    var prop = type.GetProperty( trigger );
                    Func<bool> triggerFunc = null;
                        
                    if ( prop.PropertyType == typeof( bool ) )
                    { triggerFunc = DelegateGenerator.CreateGetter<bool>( model, type.GetProperty( trigger ) ); }
                    if ( switches.ContainsKey( trigger ) )
                    { triggerFunc = switches[trigger]; }

                    if ( triggerFunc == null )
                    {
                        throw new InvalidOperationException( string.Format( Resources.InvalidEventTrigger,
                                                                            trigger,
                                                                            method.Name ) );
                    }

                    triggers.Add( triggerFunc );
                }

                Func<bool, bool, bool> compositionFunc;
                if ( eventHandler.Attribute.TriggerComposition == CompositionType.And )
                { compositionFunc = ( a, b ) => a && b; }
                else if ( eventHandler.Attribute.TriggerComposition == CompositionType.Or )
                { compositionFunc = ( a, b ) => a || b; }
                else
                {
                    throw new InvalidOperationException( string.Format( Resources.UnknownTriggerComposition,
                                                                        method.Name ) );
                }

                Func<bool> compositeTrigger = ( ) =>
                {
                    var result = triggers[0]( );
                    foreach ( var trigger in triggers.Skip( 1 ) )
                    { result = compositionFunc( result, trigger( ) ); }
                    return result;
                };

                bool reenterable;

                if ( !DelegateGenerator.IsAsync( method ) )
                {
                    Action performAction = GetMethodAction( model, 
                                                            method, 
                                                            out reenterable );
                    performAction = AddExceptionHandling( performAction,
                                                          method,
                                                          model,
                                                          loop,
                                                          treatMethodsAsCritical,
                                                          logExceptionsByDefault );
                    if ( reenterable )
                    {
                        eventHandlersToAdd
                            .Add( new Tuple<Func<bool>, OrderedAction>( compositeTrigger,
                                                                        new OrderedAction( performAction, 
                                                                                           eventHandler.Priority ) ) );
                    }
                    else
                    {
                        nonReenterableEventHandlers
                            .Add( method.Name,
                                  new Tuple<Func<bool>, OrderedAction>( compositeTrigger,
                                                                        new OrderedAction( performAction,
                                                                                           eventHandler.Priority ) ) );
                    }
                }
                else
                {
                    Func<Task> performAsync = GetAsyncMethodAction( model, 
                                                                    method, 
                                                                    out reenterable );
                    performAsync = AddAsyncExceptionHandling( performAsync,
                                                              method,
                                                              model,
                                                              loop,
                                                              treatMethodsAsCritical,
                                                              logExceptionsByDefault );

                    if ( reenterable )
                    {
                        eventAsyncsToAdd
                            .Add( new Tuple<Func<bool>, 
                                            OrderedFunc<Task>>( compositeTrigger,
                                                                new OrderedFunc<Task>( performAsync,
                                                                                       eventHandler.Priority ) ) );
                    }
                    else
                    {
                        nonReenterableEventAsyncs
                            .Add( method.Name,
                                  new Tuple<Func<bool>, 
                                            OrderedFunc<Task>>( compositeTrigger,
                                                                new OrderedFunc<Task>( performAsync,
                                                                                       eventHandler.Priority ) ) );
                    }
                }
            }

            #endregion

            #region Create mutual exclusions

            // create mutual exclusions
            var exclusionAttributes = type.GetCustomAttributes<MutualExclusionAttribute>( );

            //todo: check if any need to discard methods at mutual exclusion
            foreach ( var exclusion in exclusionAttributes )
            {
                var exclusionGuard = new object( );
                //bool locked = false;
                //string enteredName = null;
                int exclusionCounter = 0;
                int current = -1;

                foreach ( var methodName in exclusion.Methods )
                {
                    // Using indexing to allow multiple entrance for the same method
                    // This delegates repeating entrances handling to non-reenterable attribute
                    int exclusionIndex = exclusionCounter++;

                    // These templates define how mutual exclusion is handled for method
                    #region Guarded function templates
                    Func<Func<Task>, Task> nonDiscardingTemplateAsync = async method =>
                    {
                        lock ( exclusionGuard )
                        {
                            while ( current != -1 && current != exclusionIndex )
                            { Monitor.Wait( exclusionGuard ); }
                            current = exclusionIndex;
                        }
                        await method( );
                        lock ( exclusionGuard )
                        {
                            current = -1;
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Func<Func<Task>, Task> discardingTemplateAsync = async method =>
                    {
                        lock ( exclusionGuard )
                        {
                            if ( current != -1 && current != exclusionIndex )
                            { return; }
                            current = exclusionIndex;
                        }
                        await method( );
                        lock ( exclusionGuard )
                        {
                            current = -1;
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Action<Action> nonDiscardingTemplate = method =>
                    {
                        lock ( exclusionGuard )
                        {
                            while ( current != -1 )
                            { Monitor.Wait( exclusionGuard ); }
                            method( );
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Action<Action> discardingTemplate = method =>
                    {
                        lock ( exclusionGuard )
                        {
                            if ( current != -1 )
                            { return; }
                            method( );
                            Monitor.Pulse( exclusionGuard );
                        }
                    };
                    #endregion Guarded function templates

                    if ( nonReenterableAsyncs.ContainsKey( methodName ) )
                    {
                        var method = nonReenterableAsyncs[methodName];
                        Func<Task> guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplateAsync( method.Func ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplateAsync( method.Func ); }

                        nonReenterableAsyncs[methodName] = new OrderedFunc<Task>( guardedMethod, 
                                                                                  method.Priority );
                    }
                    else if ( nonReenterableEventAsyncs.ContainsKey( methodName ) )
                    {
                        var pair = nonReenterableEventAsyncs[methodName];
                        var method = pair.Item2;
                        Func<Task> guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplateAsync( method.Func ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplateAsync( method.Func ); }

                        nonReenterableEventAsyncs[methodName] = 
                            new Tuple<Func<bool>, OrderedFunc<Task>>( pair.Item1, 
                                                                      new OrderedFunc<Task>( guardedMethod, 
                                                                                             method.Priority ) );
                    }
                    else if ( nonReenterableMethods.ContainsKey( methodName ) )
                    {
                        var method = nonReenterableMethods[methodName];
                        Action guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplate( method.Action ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplate( method.Action ); }

                        nonReenterableMethods[methodName] = new OrderedAction( guardedMethod,
                                                                               method.Priority );
                    }
                    else if ( nonReenterableEventHandlers.ContainsKey( methodName ) )
                    {
                        var pair = nonReenterableEventHandlers[methodName];
                        var method = pair.Item2;
                        Action guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplate( method.Action ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplate( method.Action ); }

                        nonReenterableEventHandlers[methodName] = 
                            new Tuple<Func<bool>, OrderedAction>( pair.Item1, 
                                                                  new OrderedAction( guardedMethod, 
                                                                                     method.Priority ) );
                    }
                    else
                    {
                        throw new InvalidOperationException( string.Format( Resources.NotNonReenterableMethod,
                                                                            methodName ) );
                    }
                }
            }

            #endregion

            #region Merge reenterable and guarded methods

            actionsToAdd.AddRange( nonReenterableMethods.Select( x => x.Value ) );
            asyncsToAdd.AddRange( nonReenterableAsyncs.Select( x => x.Value ) );

            eventHandlersToAdd.AddRange( nonReenterableEventHandlers.Select( x => x.Value ) );
            eventAsyncsToAdd.AddRange( nonReenterableEventAsyncs.Select( x => x.Value ) );

            actionsToAdd.AddRange( asyncsToAdd.Select( x => new OrderedAction( ( ) => x.Func( ), x.Priority ) ) );
            eventHandlersToAdd.AddRange( 
                eventAsyncsToAdd.Select( x => 
                    new Tuple<Func<bool>, OrderedAction>( x.Item1, 
                                                          new OrderedAction( ( ) => x.Item2.Func( ), 
                                                                             x.Item2.Priority ) ) ) );

            #endregion

            #region Register generated actions and events

            foreach ( var shutdownEvent in shutdownEventsToAdd )
            {
                loop.RegisterShutdownEvent( shutdownEvent );
            }

            foreach ( var orderedAction in actionsToAdd )
            {
                loop.RegisterAction( orderedAction.Action, orderedAction.Priority );
            }

            foreach ( var eventHandler in eventHandlersToAdd )
            {
                loop.RegisterEvent( eventHandler.Item1, 
                                    eventHandler.Item2.Action, 
                                    eventHandler.Item2.Priority );
            }

            if ( hasCriticalActions || treatMethodsAsCritical )
            { loop.CheckFatal = true; }

            #endregion
        }

        /// <summary>
        /// Creates getters for all method parameters marked with <see cref="FromSourceAttribute"/>.
        /// Throws an <see cref="InvalidOperationException"/> if some parameters are not marked with attribute 
        /// or there is no source property for them, and <see cref="InvalidCastException"/> if parameter type is not equal to 
        /// property type.
        /// </summary>
        /// <param name="target">Object-owner of method and parameters sources</param>
        /// <param name="method">Method to parse its parameters</param>
        /// <returns>
        /// <see cref="List{Func{object}}"/> of parameters getters.
        /// The order of getters corresponds to order of parameters in method signature.
        /// </returns>
        private static List<Func<object>> GetParametersSources( object target, MethodInfo method )
        {
            var parameterGetters = new List<Func<object>>( );
            var type = target.GetType( );

            foreach ( var parameter in method.GetParameters( ) )
            {
                var sourceAttribute = parameter.GetCustomAttribute<FromSourceAttribute>( );
                if ( sourceAttribute == null )
                {
                    throw new InvalidOperationException( string.Format( Resources.NoParameterSource,
                                                                        parameter.Name,
                                                                        method.Name ) );
                }

                var sourceProperty = type.GetProperty( sourceAttribute.SourceName );

                if ( sourceProperty == null )
                {
                    throw new InvalidOperationException( string.Format( Resources.SourceNotFound,
                                                                        sourceAttribute.SourceName,
                                                                        parameter.Name,
                                                                        method.Name ) );
                }

                if ( sourceProperty.PropertyType != parameter.ParameterType )
                {
                    throw new InvalidCastException( string.Format( Resources.SourceTypeMismatch,
                                                                   sourceAttribute.SourceName,
                                                                   parameter.Name,
                                                                   method.Name ) );
                }

                parameterGetters.Add( DelegateGenerator.CreateGetter( target, sourceProperty ) );
            }

            return parameterGetters;
        }

        /// <summary>
        /// Creates method performing model action to call in event loop.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, not reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <param name="reenterable">Returns if action is defined as reenterable</param>
        /// <returns><see cref="Action"/> object to perform the action</returns>
        private static Action GetMethodAction( object target, 
                                               MethodInfo method, 
                                               out bool reenterable )
        {
            if ( method.ReturnType != typeof( void ) )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidAction,
                                                                    method.Name ) );
            }

            if ( method.GetCustomAttribute<EventHandlerAttribute>( ) != null
                 && method.GetCustomAttribute<ActionAttribute>( ) != null )
            {
                throw new InvalidOperationException( string.Format( Resources.ActionCantBeHandler,
                                                                    method.Name ) );
            }

            var nonReenterable = method.GetCustomAttribute<NonReenterableAttribute>( );
            reenterable = nonReenterable == null;

            var parameterGetters = GetParametersSources( target, method );

            Action<object[]> callAction = DelegateGenerator.GenerateAction( target, method );

            if ( nonReenterable != null )
            {
                callAction = DelegateGenerator.MakeReenterable( callAction,
                                                                nonReenterable.DiscardRepeated );
            }

            Action performAction = ( ) =>
            {
                var argumentsArray = ( from getter in parameterGetters
                                       select getter( ) ).ToArray( );
                callAction( argumentsArray );
            };

            return performAction;
        }

        /// <summary>
        /// Wraps an action by try-catch statement. 
        /// Generates <see cref="EventLoop"/> shutdown for critical methods.
        /// </summary>
        /// <param name="action">Action to add exception handling</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing passed action</param>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="loop">
        /// Loop which will perform actions 
        /// and will be stopped if critical action throws an exception
        /// </param>
        /// <param name="treatMethodsAsCritical">
        /// If true, all methods without <see cref="CriticalAttribute"/> or <see cref="NonCriticalAttribute"/>
        /// will be treated as critical (otherwise - non-critical).
        /// </param>
        /// <returns>Action with added exception handling logic</returns>
        private static Action AddExceptionHandling( Action action,
                                                    MethodInfo method,
                                                    object target,
                                                    EventLoop loop,
                                                    bool treatMethodsAsCritical,
                                                    bool logExceptionsByDefault )
        {
            Action result;
            var critical = method.GetCustomAttribute<CriticalAttribute>( );
            var nonCritical = method.GetCustomAttribute<NonCriticalAttribute>( );

            if ( critical != null && nonCritical != null )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidCriticalAttribute,
                                                                    method.Name ) );
            }
            else if ( critical != null )
            {
                Logger logger = null;
                if ( critical.LogExceptions )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapCritical( action, 
                                                        ( ) => loop.FatalOccurred = true, 
                                                        logger );
            }
            else if ( nonCritical != null )
            {
                Logger logger = null;
                if ( nonCritical.LogExceptions )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapNonCritical( action,
                                                           logger );
            }
            else if ( treatMethodsAsCritical )
            {
                Logger logger = null;
                if ( logExceptionsByDefault )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapCritical( action,
                                                        ( ) => loop.FatalOccurred = true,
                                                        logger );
            }
            else
            {
                Logger logger = null;
                if ( logExceptionsByDefault )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapNonCritical( action,
                                                           logger );
            }

            return result;
        }

        /// <summary>
        /// Creates method performing model async action to call in event loop.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, not reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <param name="reenterable">Returns if action is defined as reenterable</param>
        /// <param name="treatMethodsAsCritical">
        /// If true, all methods without <see cref="CriticalAttribute"/> or <see cref="NonCriticalAttribute"/>
        /// will be treated as critical (otherwise - non-critical).
        /// </param>
        /// <returns><see cref="Func{Task}"/> object to perform the action</returns>
        private static Func<Task> GetAsyncMethodAction( object target,
                                                        MethodInfo method,
                                                        out bool reenterable )
        {
            if ( method.ReturnType != typeof( Task ) )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidAsyncAction,
                                                                    method.Name ) );
            }

            if ( method.GetCustomAttribute<EventHandlerAttribute>( ) != null
                 && method.GetCustomAttribute<ActionAttribute>( ) != null )
            {
                throw new InvalidOperationException( string.Format( Resources.ActionCantBeHandler,
                                                                    method.Name ) );
            }

            var nonReenterable = method.GetCustomAttribute<NonReenterableAttribute>( );
            reenterable = nonReenterable == null;

            var parameterGetters = GetParametersSources( target, method );

            Func<object[], Task> callAction = DelegateGenerator.GenerateAsyncAction( target, method );

            if ( nonReenterable != null )
            {
                callAction = DelegateGenerator.MakeAsyncReenterable( callAction,
                                                                     nonReenterable.DiscardRepeated );
            }

            Func<Task> performAction = ( ) =>
            {
                var argumentsArray = ( from getter in parameterGetters
                                       select getter( ) ).ToArray( );
                return callAction( argumentsArray );
            };

            return performAction;
        }

        /// <summary>
        /// Wraps an async action by try-catch statement. 
        /// Generates <see cref="EventLoop"/> shutdown for critical methods.
        /// </summary>
        /// <param name="async">Async action to add exception handling</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing passed action</param>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="loop">
        /// Loop which will perform actions 
        /// and will be stopped if critical action throws an exception
        /// </param>
        /// <param name="treatMethodsAsCritical">
        /// If true, all methods without <see cref="CriticalAttribute"/> or <see cref="NonCriticalAttribute"/>
        /// will be treated as critical (otherwise - non-critical).
        /// </param>
        /// <returns>Action with added exception handling logic</returns>
        private static Func<Task> AddAsyncExceptionHandling( Func<Task> async,
                                                             MethodInfo method,
                                                             object target,
                                                             EventLoop loop,
                                                             bool treatMethodsAsCritical,
                                                             bool logExceptionsByDefault )
        {
            Func<Task> result;
            var critical = method.GetCustomAttribute<CriticalAttribute>( );
            var nonCritical = method.GetCustomAttribute<NonCriticalAttribute>( );

            if ( critical != null && nonCritical != null )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidCriticalAttribute,
                                                                    method.Name ) );
            }
            else if ( critical != null )
            {
                Logger logger = null;
                if ( critical.LogExceptions )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapAsyncCritical( async,
                                                             ( ) => loop.FatalOccurred = true,
                                                             logger );
            }
            else if ( nonCritical != null )
            {
                Logger logger = null;
                if ( nonCritical.LogExceptions )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapAsyncNonCritical( async,
                                                                logger );
            }
            else if ( treatMethodsAsCritical )
            {
                Logger logger = null;
                if ( logExceptionsByDefault )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapAsyncCritical( async,
                                                             ( ) => loop.FatalOccurred = true,
                                                             logger );
            }
            else
            {
                Logger logger = null;
                if ( logExceptionsByDefault )
                { logger = LogManager.GetLogger( target.GetType( ).Name ); }
                result = ExceptionHandler.WrapAsyncNonCritical( async,
                                                                logger );
            }

            return result;
        }
    }
}
