using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Provides methods for injecting exception-handling logic to model methods.
    /// </summary>
    internal class ExceptionHandler
    {
        private static readonly Task CompletedTask = Task.FromResult( -1 );

        /// <summary>
        /// Addes try-catch to specified method. If exception is thrown, 
        /// calls specified handler.
        /// </summary>
        /// <param name="method">Method to add exception-handling logic.</param>
        /// <param name="notifyOfException">Delegate that will be called if exception was thrown.</param>
        /// <param name="logger">If specified, exception will be passed to this logger.</param>
        /// <returns>The same method wrapped by with try-catch statement.</returns>
        public static Action WrapCritical( Action method, Action notifyOfException, Logger logger = null )
        {
            return ( ) =>
            {
                try
                {
                    method( );
                }
                catch ( Exception e )
                {
                    if ( logger != null )
                        logger.Log( LogLevel.Fatal, e );
                    notifyOfException( );
                }
            };
        }

        /// <summary>
        /// Addes try-catch to specified method. Only loggs exception if logger specified.
        /// </summary>
        /// <param name="method">Method to add exception-handling logic.</param>
        /// <param name="logger">If specified, exception will be passed to this logger.</param>
        /// <returns>The same method wrapped by with try-catch statement.</returns>
        public static Action WrapNonCritical( Action method, Logger logger = null )
        {
            if ( logger != null )
            {
                return ( ) =>
                {
                    try
                    {
                        method( );
                    }
                    catch ( Exception e )
                    {
                        logger.Log( LogLevel.Error, e );
                    }
                };
            }
            else
            {
                return ( ) => { try { method( ); } catch ( Exception ) { } };
            }
        }

        /// <summary>
        /// Addes try-catch to specified method. If exception is thrown, 
        /// calls specified method.
        /// </summary>
        /// <param name="method">Method to add exception-handling logic.</param>
        /// <param name="notifyOfException">Delegate that will be called if exception was thrown.</param>
        /// <param name="logger">If specified, exception will be passed to this logger.</param>
        /// <returns>The same method wrapped by with try-catch statement.</returns>
        public static Func<Task> WrapAsyncCritical( Func<Task> method, 
                                                    Action notifyOfException, 
                                                    Logger logger = null )
        {
            return async ( ) =>
            {
                try
                {
                    await method( );
                }
                catch ( Exception e )
                {
                    if ( logger != null )
                        logger.Log( LogLevel.Fatal, e );
                    notifyOfException( );
                }
                // Unreachable because of Exit
                await CompletedTask;
            };
        }

        /// <summary>
        /// Addes try-catch to specified method. Only loggs exception if logger specified.
        /// If exception is thrown, method will return empty completed task.
        /// </summary>
        /// <param name="method">Method to add exception-handling logic.</param>
        /// <param name="logger">If specified, exception will be passed to this logger.</param>
        /// <returns>The same method wrapped by with try-catch statement.</returns>
        public static Func<Task> WrapAsyncNonCritical( Func<Task> method, Logger logger = null )
        {
            if ( logger != null )
            {
                return async ( ) =>
                {
                    try
                    {
                        await method( );
                    }
                    catch ( Exception e )
                    {
                        logger.Log( LogLevel.Error, e );
                        await CompletedTask;
                    }
                };
            }
            else
            {
                return async ( ) =>
                {
                    try
                    {
                        await method( );
                    }
                    catch ( Exception )
                    {
                        await CompletedTask;
                    }
                };
            }
        }
    }
}
