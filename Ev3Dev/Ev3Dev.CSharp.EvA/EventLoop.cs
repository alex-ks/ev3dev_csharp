using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    public class EventLoop
    {
        private Dictionary<Func<bool>, OrderedAction> _eventHandlers =
            new Dictionary<Func<bool>, OrderedAction>( );

        private List<OrderedAction> _actions = new List<OrderedAction>( );

        private List<Func<bool>> _shutdownEvents = new List<Func<bool>>( );

        public void RegisterEvent( Func<bool> trigger, Action handler, int priority = int.MinValue )
        {
            _eventHandlers.Add( trigger, new OrderedAction( handler, priority ) );
        }

        public void RegisterAction( Action action, int priority = int.MinValue )
        {
            _actions.Add( new OrderedAction( action, priority ) );
        }

        public void RegisterShutdownEvent( Func<bool> sEvent )
        {
            _shutdownEvents.Add( sEvent );
        }

        public void Reset( )
        {
            _eventHandlers.Clear( );
            _shutdownEvents.Clear( );
            _actions.Clear( );
        }

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

        public void Start( TimeSpan cooldown )
        {
            Start( ( int )cooldown.TotalMilliseconds );
        }
    }
}
