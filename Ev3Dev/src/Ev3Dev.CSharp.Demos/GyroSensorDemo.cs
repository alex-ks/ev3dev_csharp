using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ev3Dev.CSharp.BasicDevices;
using Ev3Dev.CSharp.BasicDevices.Sensors;

namespace Ev3Dev.CSharp.Demos
{
	public class GyroSensorDemo
	{
		public static void Main( )
		{
			Console.WriteLine( "Connecting to sensor..." );
			using ( var sensor = new GyroSensor( InputPort.In2 ) )
			{
				sensor.Mode = GyroSensorMode.AngleAndRotationSpeed;
				Console.CursorVisible = false;
				while ( true )
				{
					Console.WriteLine( $"{sensor.Angle}      " );
					Console.Write( $"{sensor.RotationSpeed}      " );
					Console.SetCursorPosition( 0, Console.CursorTop - 1 );
					Thread.Sleep( 100 );
				}
			}
		}
	}
}
