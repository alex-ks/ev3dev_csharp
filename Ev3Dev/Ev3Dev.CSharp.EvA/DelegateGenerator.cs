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
    /// <summary>
    /// Provides methods for delegates creation from reflection.
    /// </summary>
    internal class DelegateGenerator
    {
        /// <summary>
        /// Creates <see cref="Func{TResult}"/> to access property value.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, otherwise via reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Object-owner of a property</param>
        /// <param name="property">Property for wich value the getter will be created</param>
        /// <returns><see cref="Func{TResult}"/> representing property getter</returns>
        public static Func<T> CreateGetter<T>( object target, PropertyInfo property )
        {
            return Delegate.CreateDelegate( typeof( Func<T> ),
                                            target,
                                            property.GetMethod ) as Func<T>;
        }

        /// <summary>
        /// Creates <see cref="Func{Object}"/> to access property value of unknown type.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, otherwise via reflection.
        /// </summary>
        /// <param name="target">Object-owner of a property</param>
        /// <param name="property">Property for wich value the getter will be created</param>
        /// <returns><see cref="Func{TResult}"/> representing property getter</returns>
        public static Func<object> CreateGetter( object target, PropertyInfo property )
        {
            return ReflectionToDelegateConverter.CreateFuncZeroArg( target, property.GetMethod );
        }

        /// <summary>
        /// Creates <see cref="System.Action"/> to call method with no return value.
        /// Method parameters are to be passed as array of objects.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called directly via delegates, otherwise via reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <param name="reenterable">Returns if action is defined as reenterable</param>
        /// <returns><see cref="Action"/> object to perform the action</returns>
        public static Action<object[]> GenerateAction( object target, MethodInfo method )
        {
            if ( method.ReturnType != typeof( void ) )
            {
                throw new InvalidOperationException( string.Format( Resources.InvalidAction,
                                                                    method.Name ) );
            }

            Action<object[]> callAction;
            var parametersCount = method.GetParameters( ).Length;

            // create direct delegate invocation if argument count is small
            if ( parametersCount == 0 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateActionZeroArg( target, method );
                callAction = ( args ) => convertedAct( );
            }
            else if ( parametersCount == 1 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction1Arg( target, method );
                callAction = ( args ) => convertedAct( args[0] );
            }
            else if ( parametersCount == 2 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction2Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1] );
            }
            else if ( parametersCount == 3 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction3Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1],
                                                       args[2] );
            }
            else if ( parametersCount == 4 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction4Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1],
                                                       args[2],
                                                       args[3] );
            }
            else if ( parametersCount == 5 )
            {
                var convertedAct = ReflectionToDelegateConverter.CreateAction5Args( target, method );
                callAction = ( args ) => convertedAct( args[0],
                                                       args[1],
                                                       args[2],
                                                       args[3],
                                                       args[4] );
            }
            else // otherwise perform slow invocation through reflection
            { callAction = ( args ) => method.Invoke( target, args ); }

            return callAction;
        }

        /// <summary>
        /// Creates function to call an async method.
        /// Method parameters are to be passed as array of objects.
        /// Performs optimizations for actions with parameters count less or equal 5 - 
        /// in this case methods are called via delegates, otherwise via reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <returns><see cref="Func{Task}"/> object to perform the action</returns>
        public static Func<object[], Task> GenerateAsyncAction( object target, MethodInfo method )
        {
            Func<object[], Task> callAction;

            var parametersCount = method.GetParameters( ).Length;

            // create direct delegate invocation if argument count is small
            if ( parametersCount == 0 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFuncZeroArg( target, method );
                callAction = ( args ) => convertedFunc( ) as Task;
            }
            else if ( parametersCount == 1 )
            {
                callAction = ( args ) =>
                  ReflectionToDelegateConverter.CreateFunc1Arg( target, method )( args[0] ) as Task;
            }
            else if ( parametersCount == 2 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc2Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1] ) as Task;
            }
            else if ( parametersCount == 3 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc3Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2] ) as Task;
            }
            else if ( parametersCount == 4 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc4Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2],
                                                        args[3] ) as Task;
            }
            else if ( parametersCount == 5 )
            {
                var convertedFunc = ReflectionToDelegateConverter.CreateFunc5Args( target, method );
                callAction = ( args ) => convertedFunc( args[0],
                                                        args[1],
                                                        args[2],
                                                        args[3],
                                                        args[4] ) as Task;
            }
            else // otherwise perform slow invocation through reflection
            { callAction = ( args ) => method.Invoke( target, args ) as Task; }

            return callAction;
        }

        /// <summary>
        /// Creates wrapper to safely call method from multiple threads, 
        /// using <see cref="Monitor.Enter(object)"/> for mutual exclusion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">Method to make reenterable</param>
        /// <param name="discardRepeated">
        /// If true, when one thread calls methods, all other calls will be discarded until first caller exits.
        /// Otherwise, all other calls will wait for first call execution end.
        /// </param>
        /// <returns></returns>
        public static Action<T> MakeReenterable<T>( Action<T> action, bool discardRepeated )
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

        /// <summary>
        /// Creates wrapper to safely call async method from multiple threads, 
        /// using <see cref="Monitor.Enter(object)"/> for mutual exclusion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">Method to make reenterable</param>
        /// <param name="discardRepeated">
        /// If true, when one thread calls methods, all other calls will be discarded until first caller exits.
        /// Otherwise, all other calls will wait for first call execution end.
        /// </param>
        /// <returns></returns>
        public static Func<T, Task> MakeAsyncReenterable<T>( Func<T, Task> action, bool discardRepeated )
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

        /// <summary>
        /// Checks if method represented by <see cref="MethodInfo"/> is async.
        /// </summary>
        /// <param name="method">Method to check for async-ness</param>
        /// <returns>True if method is marked as async, false otherwise</returns>
        public static bool IsAsync( MethodInfo method )
        {
            return method.GetCustomAttribute<AsyncStateMachineAttribute>( ) != null;
        }
    }
}
