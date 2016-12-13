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

        private struct OrderedAction
        {
            public Action Action { get; }
            public int Priority { get; }

            public OrderedAction( Action action, int priority )
            {
                Action = action;
                Priority = priority;
            }
        }

        private struct OrderedFunc<T>
        {
            public Func<T> Func { get; }
            public int Priority { get; }

            public OrderedFunc( Func<T> func, int priority )
            {
                Func = func;
                Priority = priority;
            }
        }

        public static void RegisterModel( this EventLoop loop, object model, bool allowEndless = false )
        {
            var type = model.GetType( );

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

                var getter = CreateGetter( model, prop );
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
                { shutdownEvent = CreateGetter<bool>( model, prop ); }
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

            // to keep actions that potentially need mutual exclusion
            var nonReenterableMethods = new Dictionary<string, OrderedAction>( );
            var nonReenterableAsyncs = new Dictionary<string, OrderedFunc<Task>>( );

            foreach ( var pair in actions )
            {
                bool reenterable;
                var method = pair.Method;

                if ( !IsAsync( method ) )
                {
                    Action performAction = GetMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    { actionsToAdd.Add( new OrderedAction( performAction, pair.Priority ) ); }
                    else
                    { nonReenterableMethods.Add( method.Name, new OrderedAction( performAction, pair.Priority ) ); }
                }
                else
                {
                    Func<Task> performAsync = GetAsyncMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    { actionsToAdd.Add( new OrderedAction( ( ) => performAsync( ), pair.Priority ) ); }
                    else
                    { nonReenterableAsyncs.Add( method.Name, new OrderedFunc<Task>( performAsync, pair.Priority ) ); }
                }
            }

            // parse event handlers and their triggers
            var eventHandlersToAdd = new List<Tuple<Func<bool>, OrderedAction>>( );
            
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
                    { triggerFunc = CreateGetter<bool>( model, type.GetProperty( trigger ) ); }
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
                    Func<Task> performAsync = GetAsyncMethodAction( model, method, out reenterable );
                    if ( reenterable )
                    {
                        eventHandlersToAdd
                            .Add( new Tuple<Func<bool>, OrderedAction>( compositeTrigger,
                                                                        new OrderedAction( ( ) => performAsync( ),
                                                                                           eventHandler.Priority ) ) );
                    }
                    else
                    {
                        nonReenterableEventAsyncs
                            .Add( method.Name,
                                  new Tuple<Func<bool>, OrderedFunc<Task>>( compositeTrigger,
                                                                            new OrderedFunc<Task>( performAsync,
                                                                                                   eventHandler.Priority ) ) );
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

            // merging reenterable and guarded methods

            actionsToAdd.AddRange( nonReenterableMethods.Select( x => x.Value ) );
            actionsToAdd.AddRange( nonReenterableAsyncs.Select( x => 
                new OrderedAction( ( ) => x.Value.Func( ), x.Value.Priority ) ) );
            eventHandlersToAdd.AddRange( nonReenterableEventHandlers.Select( x => x.Value ) );
            eventHandlersToAdd.AddRange( nonReenterableEventAsyncs.Select( x =>
                new Tuple<Func<bool>, OrderedAction>( x.Value.Item1,
                                                      new OrderedAction( ( ) => x.Value.Item2.Func( ),
                                                                         x.Value.Item2.Priority ) ) ) );

            // registering generated actions and events

            foreach ( var shutdownEvent in shutdownEventsToAdd )
            {
                loop.RegisterShutdownEvent( shutdownEvent );
            }

            foreach ( var action in actionsToAdd.OrderByDescending( x => x.Priority )
                                                .Select( x => x.Action ) )
            {
                loop.RegisterAction( action );
            }

            foreach ( var eventHandler in eventHandlersToAdd.OrderByDescending( x => x.Item2.Priority ) )
            {
                loop.RegisterEvent( eventHandler.Item1, eventHandler.Item2.Action );
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
            {
                var convertedAct = ReflectionToDelegateConverter.CreateActionZeroArg( target, method );
                callAction = ( args ) => convertedAct( );
            }
            else if ( parameterGetters.Count == 1 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction1Arg( target, method );
                callAction = ( args ) => convertedAct( args[0] );
            }
            else if ( parameterGetters.Count == 2 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction2Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1] );
            }
            else if ( parameterGetters.Count == 3 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction3Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1],
                                                       args[2] );
            }
            else if ( parameterGetters.Count == 4 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction4Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1],
                                                       args[2],
                                                       args[3] );
            }
            else if ( parameterGetters.Count == 5 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction5Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
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
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFuncZeroArg( target, method );
                callAction = ( args ) => convertedFunc( ) as Task;
            }
            else if ( parameterGetters.Count == 1 )
            { callAction = ( args ) => 
                ReflectionToDelegateConverter.CreateFunc1Arg( target, method )( args[0] ) as Task; }
            else if ( parameterGetters.Count == 2 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc2Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1] ) as Task;
            }
            else if ( parameterGetters.Count == 3 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc3Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2] ) as Task;
            }
            else if ( parameterGetters.Count == 4 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc4Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2],
                                                        args[3] ) as Task;
            }
            else if ( parameterGetters.Count == 5 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc5Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2],
                                                        args[3],
                                                        args[4] ) as Task;
            }
            else
            { callAction = ( args ) => method.Invoke( target, args ) as Task; }

            if ( nonReenterable != null )
            {
                callAction = MakeAsyncReenterable( callAction,
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
