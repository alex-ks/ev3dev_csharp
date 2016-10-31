﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.ControlFlow
{
    public static class ModelParser
    {
        public static void RegisterModel( this EventLoop loop, object model )
        {
            var type = model.GetType( );

            var shutdownEvents = from prop in type.GetProperties( )
                                 let attribute = prop.GetCustomAttribute<ShutdownEventAttribute>( )
                                 where attribute != null
                                 select prop;

            var eventsToAdd = new List<Func<bool>>( );

            foreach ( var prop in shutdownEvents )
            {
                if ( prop.PropertyType != typeof( bool ) )
                { throw new InvalidOperationException( Resources.InvalidShutdownEvent ); }

                var shutdownEvent = CreateGetter<bool>( model, prop );
                eventsToAdd.Add( shutdownEvent );
            }
            
            var actions = from method in type.GetMethods( )
                          let attribute = method.GetCustomAttribute<ActionAttribute>( )
                          where attribute != null
                          select method;

            var actionsToAdd = new List<Action>( );
            var synchronousMethods = new Dictionary<string, Action>( );

            foreach ( var method in actions )
            {
                bool reenterable;

                Action performAction = !IsAsync( method ) ?
                    GetMethodAction( model, method, out reenterable ) :
                    GetAsyncMethodAction( model, method, out reenterable );

                if ( !reenterable )
                { synchronousMethods.Add( method.Name, performAction ); }
                else
                { actionsToAdd.Add( performAction ); }
            }

            var synchronousEvents = new Dictionary<string, Func<bool>>( );

            var eventHandlers = from method in type.GetMethods( )
                                let attribute = method.GetCustomAttribute<EventHandlerAttribute>( )
                                where attribute != null
                                select new { Method = method, Attribute = attribute };

            foreach ( var eventHandler in eventHandlers )
            {
                //todo: implement event handlers
            }

            //todo: implement mutual exclusion
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

            if ( method.GetCustomAttribute<EventHandlerAttribute>( ) != null )
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
        /// <returns><see cref="Action"/> object to perform the action</returns>
        private static Action GetAsyncMethodAction( object target,
                                                    MethodInfo method,
                                                    out bool reenterable )
        {
            if ( method.ReturnType != typeof( Task ) )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidAsyncAction,
                                                                    method.Name ) );
            }

            if ( method.GetCustomAttribute<EventHandlerAttribute>( ) != null )
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

            Action performAction = ( ) =>
            {
                var argumentsArray = ( from getter in parameterGetters
                                       select getter( ) ).ToArray( );
                callAction( argumentsArray );
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