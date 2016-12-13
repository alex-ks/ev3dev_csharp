using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.Demos
{
    [MutualExclusion( nameof( WriteFirstAsync ), nameof( WriteSecondAsync ) )]
    public class StreamWriterModel : IDisposable
    {
        public const int WaitTime = 100;

        private StreamWriter writer;
        private int counter = 0;
        
        public int ActivationCount { get; private set; }

        [ShutdownEvent]
        public bool LimitReached => counter == 10;
        public bool LimitAlmostReached => counter == 9;

        public StreamWriterModel( Stream stream )
        {
            writer = new StreamWriter( stream );
        }

        [Action]
        [Priority( 10 )]
        [NonReenterable( DiscardRepeated = false )]
        public async Task WriteFirstAsync( )
        {
            await writer.WriteLineAsync( "Hello, world" );
            await writer.FlushAsync( );
            ++ActivationCount;
            Thread.Sleep( WaitTime );
        }

        [Action]
        [Priority( 5 )]
        [NonReenterable]
        public async Task WriteSecondAsync( )
        {
            await writer.WriteLineAsync( "Hello, world 2" );
            await writer.FlushAsync( );
            Thread.Sleep( WaitTime );
            return;
        }

        [Action]
        public void Increment( )
        {
            ++counter;
        }

        [EventHandler( nameof( LimitAlmostReached ) )]
        public void AlertShutdown( [FromSource( nameof( ActivationCount ) )]int activationCount )
        {
            writer.WriteLine( "Limit almost reached" );
            writer.WriteLine( activationCount );
        }

        public void Dispose( )
        {
            writer.Dispose( );
        }
    }

    public class EvaDemo
    {
        public static void Main( )
        {
            var loop = new EventLoop( );
            using ( var model = new StreamWriterModel( Console.OpenStandardOutput( ) ) )
            {
                loop.RegisterModel( model );
                // If we make cooldown more than WaitTime, method with lower priority will never be called
                loop.Start( millisecondsCooldown: StreamWriterModel.WaitTime + 20 );
                Thread.Sleep( 2000 );
            }
        }
    }
}
