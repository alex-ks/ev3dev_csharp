using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ev3Dev.CSharp.BasicDevices;

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
				motor.Run( 1.0f, 75 );
				motor.WaitForStop( );

				Console.Out.WriteLine( "One second run..." );
				motor.RunTimed( 1000, 75 );
				motor.WaitForStop( );

				Console.Out.WriteLine( "1.5 seconds back..." );
				motor.RunForever( -75 );
				Thread.Sleep( 1500 );
				motor.Stop( );
			}

			Console.Out.WriteLine( "Finish" );
		}
	}
}
