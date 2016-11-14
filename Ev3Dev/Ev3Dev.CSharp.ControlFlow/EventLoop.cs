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
        private Dictionary<Func<bool>, Action> _eventHandlers =
            new Dictionary<Func<bool>, Action>( );

        private SortedSet<Action> _actions = new SortedSet<Action>( );

        private SortedSet<Func<bool>> _shutdownEvents = new SortedSet<Func<bool>>( );

        public void RegisterEvent( Func<bool> trigger, Action handler )
        {
            _eventHandlers.Add( trigger, handler );
        }

        public void RegisterAction( Action action )
        {
            _actions.Add( action );
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

        private void PerformLoopIteration( out bool shutdown )
        {
            foreach ( var needToShutdown in _shutdownEvents )
            {
                if ( needToShutdown( ) )
                {
                    shutdown = true;
                    return;
                }
            }

            foreach ( var performAction in _actions )
            {
                performAction( );
            }

            foreach ( var eventPair in _eventHandlers )
            {
                if ( eventPair.Key( ) )
                { eventPair.Value( ); }
            }

            shutdown = false;
        }

        public void StartLoop( int millisecondsCooldown = 0 )
        {
            bool needToShutdown = false;
            while ( !needToShutdown )
            {
                PerformLoopIteration( out needToShutdown );

                if ( millisecondsCooldown != 0 )
                { Thread.Sleep( millisecondsCooldown ); }
            }
        }

        public void StartLoop( TimeSpan cooldown )
        {
            bool needToShutdown = false;
            while ( !needToShutdown )
            {
                PerformLoopIteration( out needToShutdown );

                if ( cooldown != TimeSpan.Zero )
                { Thread.Sleep( cooldown ); }
            }
        }
    }
}
