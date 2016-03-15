using System;
using System.Threading;
using Ev3Dev.CSharp.BasicDevices;
using Ev3Dev.CSharp.BasicDevices.Motors;

namespace Ev3Dev.CSharp.Demos
{
	public class MotorDemo
	{
		public static void Main( )
		{
			Console.Out.WriteLine( "Connecting to motor..." );
			using ( var motor = new LargeMotor( OutputPort.OutB ) )
			{
				Console.Out.WriteLine( "One rotation..." );
				motor.Run( rotations: 1.0f, speed: 75 ).Wait( );

				Console.Out.WriteLine( "One second run..." );
				motor.RunTimed( ms: 1000, speed: 75 ).Wait( );

				Console.Out.WriteLine( "1.5 seconds back..." );
				motor.RunForever( speed: -75 );
				Thread.Sleep( 1500 );
				motor.Stop( );
			}

			Console.Out.WriteLine( "Finish" );
		}
	}
}
