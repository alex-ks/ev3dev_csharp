using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    public static class ModelParser
    {
        private struct EventHandler
        {
            public Func<bool> Trigger { get; set; }
            public Action Handler { get; set; }
        }

        public static void RegisterModel( this EventLoop loop, object model )
        {
            var type = model.GetType( );

            var shutdownEvents = from prop in type.GetProperties( )
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>( )
                                 where attribute != null
                                 select prop;

            var eventsToAdd = new List<Func<bool>>( );

            // parse events that will cause loop shutdown
            foreach ( var prop in shutdownEvents )
            {
                if ( prop.PropertyType != typeof( bool ) )
                { throw new InvalidOperationException( Resources.InvalidShutdownEvent ); }

                var shutdownEvent = CreateGetter<bool>( model, prop );
                eventsToAdd.Add( shutdownEvent );
            }

            // parse actions that will be performed on each iteration
            var actions = from method in type.GetMethods( )
                          let attribute = method.GetCustomAttribute<ActionAttribute>( )
                          where attribute != null
                          select method;

            var actionsToAdd = new List<Action>( );

            // to keep actions that potentially need mutual exclusion
            var nonReenterableMethods = new Dictionary<string, Action>( );
            var nonReenterableAsyncs = new Dictionary<string, Func<Task>>( );

            foreach ( var method in actions )
            {
                bool reenterable;

                if ( !IsAsync( method ) )
                {
                    Action performAction = GetMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    { actionsToAdd.Add( performAction ); }
                    else
                    { nonReenterableMethods.Add( method.Name, performAction ); }
                }
                else
                {
                    Func<Task> performAsync = GetAsyncMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    { actionsToAdd.Add( ( ) => performAsync( ) ); }
                    else
                    { nonReenterableAsyncs.Add( method.Name, performAsync ); }
                }
            }

            // parse event handlers and their triggers
            var eventHandlersToAdd = new List<Tuple<Func<bool>, Action>>( );
            
            // to keep handlers that potentially need mutual exclusion
            var nonReenterableEventHandlers = new Dictionary<string, Tuple<Func<bool>, Action>>( );
            var nonReenterableEventAsyncs = new Dictionary<string, Tuple<Func<bool>, Func<Task>>>( );

            var eventHandlers = from method in type.GetMethods( )
                                let attribute = method.GetCustomAttribute<EventHandlerAttribute>( )
                                where attribute != null
                                select new { Method = method, Attribute = attribute };
            
            foreach ( var eventHandler in eventHandlers )
            {
                var method = eventHandler.Method;
                var triggers = new List<Func<bool>>( );

                if ( eventHandler.Attribute.Triggers.Length == 0 )
                {
                    throw new InvalidOperationException( string.Format( Resources.InvalidEventTriggerCount,
                                                                        method.Name ) );
                }

                // get trigger flags
                foreach ( var trigger in eventHandler.Attribute.Triggers )
                {
                    try
                    {
                        var triggerFunc = CreateGetter<bool>( model, type.GetProperty( trigger ) );
                        triggers.Add( triggerFunc );
                    }
                    catch ( ArgumentException )
                    {
                        throw new InvalidOperationException( string.Format( Resources.InvalidEventTrigger,
                                                                            trigger,
                                                                            method.Name ) );
                    }
                }

                Func<bool, bool, bool> compositionFunc;
                if ( eventHandler.Attribute.TriggerComposition == CompositionType.AND )
                { compositionFunc = ( a, b ) => a && b; }
                else if ( eventHandler.Attribute.TriggerComposition == CompositionType.OR )
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

                if ( !IsAsync( method ) )
                {
                    Action performAction = GetMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    {
                        eventHandlersToAdd.Add( new Tuple<Func<bool>, Action>( compositeTrigger,
                                                                               performAction ) );
                    }
                    else
                    {
                        nonReenterableEventHandlers.Add( method.Name,
                                                         new Tuple<Func<bool>, Action>( compositeTrigger,
                                                                                        performAction ) );
                    }
                }
                else
                {
                    Func<Task> performAsync = GetAsyncMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    {
                        eventHandlersToAdd.Add( new Tuple<Func<bool>, Action>( compositeTrigger,
                                                                               ( ) => performAsync( ) ) );
                    }
                    else
                    {
                        nonReenterableEventAsyncs.Add( method.Name,
                                                       new Tuple<Func<bool>, Func<Task>>( compositeTrigger,
                                                                                          performAsync ) );
                    }
                }
            }

            // create mutual exclusions
            var exclusionAttributes = type.GetCustomAttributes<MutualExclusionAttribute>( );

            //todo: check if any need to discard methods at mutual exclusion
            foreach ( var exclusion in exclusionAttributes )
            {
                var exclusionGuard = new object( );
                bool locked = false;
                foreach ( var methodName in exclusion.Methods )
                {
                    #region Guarded function templates
                    Func<Func<Task>, Task> nonDiscardingTemplateAsync = async method =>
                    {
                        lock ( exclusionGuard )
                        {
                            while ( locked )
                            { Monitor.Wait( exclusionGuard ); }
                            locked = true;
                        }
                        await method( );
                        lock ( exclusionGuard )
                        {
                            locked = false;
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Func<Func<Task>, Task> discardingTemplateAsync = async method =>
                    {
                        lock ( exclusionGuard )
                        {
                            if ( locked )
                            { return; }
                            locked = true;
                        }
                        await method( );
                        lock ( exclusionGuard )
                        {
                            locked = false;
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Action<Action> nonDiscardingTemplate = method =>
                    {
                        lock ( exclusionGuard )
                        {
                            while ( locked )
                            { Monitor.Wait( exclusionGuard ); }
                            method( );
                            Monitor.Pulse( exclusionGuard );
                        }
                    };

                    Action<Action> discardingTemplate = method =>
                    {
                        lock ( exclusionGuard )
                        {
                            if ( locked )
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
                        { guardedMethod = ( ) => discardingTemplateAsync( method ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplateAsync( method ); }

                        nonReenterableAsyncs[methodName] = guardedMethod;
                    }
                    else if ( nonReenterableEventAsyncs.ContainsKey( methodName ) )
                    {
                        var pair = nonReenterableEventAsyncs[methodName];
                        var method = pair.Item2;
                        Func<Task> guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplateAsync( method ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplateAsync( method ); }

                        nonReenterableEventAsyncs[methodName] = 
                            new Tuple<Func<bool>, Func<Task>>( pair.Item1, guardedMethod );
                    }
                    else if ( nonReenterableMethods.ContainsKey( methodName ) )
                    {
                        var method = nonReenterableMethods[methodName];
                        Action guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplate( method ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplate( method ); }

                        nonReenterableMethods[methodName] = guardedMethod;
                    }
                    else if ( nonReenterableEventHandlers.ContainsKey( methodName ) )
                    {
                        var pair = nonReenterableEventHandlers[methodName];
                        var method = pair.Item2;
                        Action guardedMethod;

                        if ( exclusion.DiscardExcluded )
                        { guardedMethod = ( ) => discardingTemplate( method ); }
                        else
                        { guardedMethod = ( ) => nonDiscardingTemplate( method ); }

                        nonReenterableEventHandlers[methodName] = 
                            new Tuple<Func<bool>, Action>( pair.Item1, guardedMethod );
                    }
                    else
                    {
                        throw new InvalidOperationException( string.Format( Resources.NotNonReenterableMethod,
                                                                            methodName ) );
                    }
                }
            }

            //registering generated actions and events

            foreach ( var shutdownEvent in eventsToAdd )
            {
                loop.RegisterShutdownEvent( shutdownEvent );
            }

            foreach ( var action in actionsToAdd )
            {
                loop.RegisterAction( action );
            }

            foreach ( var eventHandler in eventHandlersToAdd )
            {
                loop.RegisterEvent( eventHandler.Item1, eventHandler.Item2 );
            }

            foreach ( var action in nonReenterableMethods )
            {
                loop.RegisterAction( action.Value );
            }

            foreach ( var asyncAction in nonReenterableAsyncs )
            {
                loop.RegisterAction( ( ) => asyncAction.Value( ) );
            }

            foreach ( var eventHandler in nonReenterableEventHandlers )
            {
                loop.RegisterEvent( eventHandler.Value.Item1, eventHandler.Value.Item2 );
            }

            foreach ( var asyncHandler in nonReenterableEventAsyncs )
            {
                loop.RegisterEvent( asyncHandler.Value.Item1, ( ) => asyncHandler.Value.Item2( ) );
            }
        }

        private static Func<T> CreateGetter<T>( object target, PropertyInfo property )
        {
            return Delegate.CreateDelegate( typeof( Func<T> ),
                                            target,
                                            property.GetMethod ) as Func<T>;
        }

        private static Func<object> CreateGetter( object target, PropertyInfo property )
        {
            return ReflectionToDelegateConverter.CreateFuncZeroArg( target, property.GetMethod );
        }

        private static bool IsAsync( MethodInfo method )
        {
            return method.GetCustomAttribute<AsyncStateMachineAttribute>( ) != null;
        }

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

                parameterGetters.Add( CreateGetter( target, sourceProperty ) );
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

            Action<object[]> callAction;

            if ( parameterGetters.Count == 0 )
            { callAction = ( args ) => ReflectionToDelegateConverter.CreateActionZeroArg( target, method ); }
            else if ( parameterGetters.Count == 1 )
            { callAction = ( args ) => ReflectionToDelegateConverter.CreateAction1Arg( target, method )( args[0] ); }
            else if ( parameterGetters.Count == 2 )
            {
                callAction = ( args ) => ReflectionToDelegateConverter.CreateAction2Args( target, method )( args[0],
                                                                                                            args[1] );
            }
            else if ( parameterGetters.Count == 3 )
            {
                callAction = ( args ) => ReflectionToDelegateConverter.CreateAction3Args( target, method )( args[0],
                                                                                                            args[1],
                                                                                                            args[2] );
            }
            else if ( parameterGetters.Count == 4 )
            {
                callAction = ( args ) => ReflectionToDelegateConverter.CreateAction4Args( target, method )( args[0],
                                                                                                            args[1],
                                                                                                            args[2],
                                                                                                            args[3] );
            }
            else if ( parameterGetters.Count == 5 )
            {
                callAction = ( args ) => ReflectionToDelegateConverter.CreateAction5Args( target, method )( args[0],
                                                                                                            args[1],
                                                                                                            args[2],
                                                                                                            args[3],
                                                                                                            args[4] );
            }
            else
            { callAction = ( args ) => method.Invoke( target, args ); }

            if ( nonReenterable != null )
            {
                callAction = MakeReenterable( callAction,
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
        /// Creates method performing model async action to call in event loop.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, not reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <param name="reenterable">Returns if action is defined as reenterable</param>
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

            Func<object[], Task> callAction;

            if ( parameterGetters.Count == 0 )
            { callAction = ( args ) => ReflectionToDelegateConverter.CreateFuncZeroArg( target, method )( ) as Task; }
            else if ( parameterGetters.Count == 1 )
            { callAction = ( args ) => 
                ReflectionToDelegateConverter.CreateFunc1Arg( target, method )( args[0] ) as Task; }
            else if ( parameterGetters.Count == 2 )
            {
                callAction = ( args ) => 
                    ReflectionToDelegateConverter.CreateFunc2Args( target, method )( args[0],
                                                                                     args[1] ) as Task;
            }
            else if ( parameterGetters.Count == 3 )
            {
                callAction = ( args ) => 
                    ReflectionToDelegateConverter.CreateFunc3Args( target, method )( args[0],
                                                                                     args[1],
                                                                                     args[2] ) as Task;
            }
            else if ( parameterGetters.Count == 4 )
            {
                callAction = ( args ) => 
                    ReflectionToDelegateConverter.CreateFunc4Args( target, method )( args[0],
                                                                                     args[1],
                                                                                     args[2],
                                                                                     args[3] ) as Task;
            }
            else if ( parameterGetters.Count == 5 )
            {
                callAction = ( args ) => 
                    ReflectionToDelegateConverter.CreateFunc5Args( target, method )( args[0],
                                                                                     args[1],
                                                                                     args[2],
                                                                                     args[3],
                                                                                     args[4] ) as Task;
            }
            else
            { callAction = ( args ) => method.Invoke( target, args ) as Task; }

            if ( nonReenterable != null )
            {
                callAction = MakeAsyncReenterable( ( args ) => method.Invoke( target, args ) as Task,
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

        private static Action<object[]> MakeReenterable( Action<object[]> action, bool discardRepeated )
        {
            var lockGuard = new object( );
            bool locked = false;

            if ( discardRepeated )
            {
                return ( args ) =>
                {
                    lock ( lockGuard )
                    {
                        if ( locked )
                        { return; }
                        locked = true;
                    }
                    action( args );
                    lock ( lockGuard )
                    { locked = false; }
                };
            }
            else
            {
                return ( args ) =>
                {
                    lock ( lockGuard )
                    {
                        while ( locked )
                        { Monitor.Wait( lockGuard ); }
                        locked = true;
                    }
                    action( args );
                    lock ( lockGuard )
                    {
                        locked = false;
                        Monitor.Pulse( lockGuard );
                    }
                };
            }
        }

        private static Func<object[], Task> MakeAsyncReenterable( Func<object[], Task> action, bool discardRepeated )
        {
            var lockGuard = new object( );
            bool locked = false;

            if ( discardRepeated )
            {
                return async ( args ) =>
                {
                    lock ( lockGuard )
                    {
                        if ( locked )
                        { return; }
                        locked = true;
                    }
                    await action( args );
                    lock ( lockGuard )
                    { locked = false; }
                };
            }
            else
            {
                return async ( args ) =>
                {
                    lock ( lockGuard )
                    {
                        while ( locked )
                        { Monitor.Wait( lockGuard ); }
                        locked = true;
                    }
                    await action( args );
                    lock ( lockGuard )
                    {
                        locked = false;
                        Monitor.Pulse( lockGuard );
                    }
                };
            }
        }
    }
}
