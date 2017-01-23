using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Represents application message loop (similar to Win32Api), 
    /// which will poll events and perform actions on each iteration
    /// </summary>
    public class EventLoop
    {
        private Dictionary<Func<bool>, OrderedAction> _eventHandlers =
            new Dictionary<Func<bool>, OrderedAction>( );

        private List<OrderedAction> _actions = new List<OrderedAction>( );

        private List<Func<bool>> _shutdownEvents = new List<Func<bool>>( );

        /// <summary>
        /// Registers event trigger and its handler.
        /// </summary>
        /// <param name="trigger">
        /// Will be called on each iteration. If trigger returns true, 
        /// event loop will call the handler
        /// </param>
        /// <param name="handler">Will be called if trigger returns true</param>
        /// <param name="priority">
        /// Indicates how early trigger will be polled during the iteration.
        /// The bigger value, the earlier trigger will be polled
        /// </param>
        public void RegisterEvent( Func<bool> trigger, Action handler, int priority = int.MinValue )
        {
            _eventHandlers.Add( trigger, new OrderedAction( handler, priority ) );
        }

        /// <summary>
        /// Registers action.
        /// </summary>
        /// <param name="action">Will be called on each iteration</param>
        /// <param name="priority">
        /// Indicates how early trigger will be polled during the iteration.
        /// The bigger value, the earlier trigger will be polled
        /// </param>
        public void RegisterAction( Action action, int priority = int.MinValue )
        {
            _actions.Add( new OrderedAction( action, priority ) );
        }

        /// <summary>
        /// Registers event which will cause loop to stop.
        /// </summary>
        /// <param name="sEvent">If true, event loop will stop iterating</param>
        public void RegisterShutdownEvent( Func<bool> sEvent )
        {
            _shutdownEvents.Add( sEvent );
        }

        /// <summary>
        /// Removes all actions and events from loop lists.
        /// </summary>
        public void Reset( )
        {
            _eventHandlers.Clear( );
            _shutdownEvents.Clear( );
            _actions.Clear( );
        }

        /// <summary>
        /// Starts event loop.
        /// </summary>
        /// <param name="millisecondsCooldown">
        /// Defines sleep period between two iterations.
        /// If equals to zero, there will be no sleep.
        /// </param>
        public void Start( int millisecondsCooldown = 0 )
        {
            bool shutdown = false;

            var eventCheckers = _eventHandlers.Select( x => new OrderedAction( ( ) =>
                                                                               {
                                                                                   if ( x.Key( ) )
                                                                                   { x.Value.Action( ); }
                                                                               }, 
                                                                               x.Value.Priority ) );

            var actionsToPerform = eventCheckers.Concat( _actions )
                                                .OrderByDescending( x => x.Priority )
                                                .Select( x => x.Action );
            
            while ( !shutdown )
            {
                foreach ( var needToShutdown in _shutdownEvents )
                {
                    if ( needToShutdown( ) )
                    {
                        shutdown = true;
                        return;
                    }
                }

                foreach ( var performAction in actionsToPerform )
                {
                    performAction( );
                }

                if ( millisecondsCooldown != 0 )
                { Thread.Sleep( millisecondsCooldown ); }
            }
        }

        /// <summary>
        /// Starts event loop.
        /// </summary>
        /// <param name="cooldown">Defines sleep period between two iterations.</param>
        public void Start( TimeSpan cooldown )
        {
            Start( ( int )cooldown.TotalMilliseconds );
        }
    }
}
