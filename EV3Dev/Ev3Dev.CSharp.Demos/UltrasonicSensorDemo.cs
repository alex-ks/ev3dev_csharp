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
	public class UltrasonicSensorDemo
	{
		public static void Main( )
		{
			Console.WriteLine( "Connecting to sensor..." );
			using ( var sensor = new UltrasonicSensor( InputPort.In2 ) )
			{
				//sensor.Mode = UltrasonicSensorMode.ContinuousMeasurementCm;
				//sensor.Mode = UltrasonicSensorMode.ContinuousMeasurementInch;
				//sensor.Mode = UltrasonicSensorMode.SingleMeasurementCm;
				//sensor.Mode = UltrasonicSensorMode.SingleMeasurementInch;
				//sensor.Mode = UltrasonicSensorMode.UsDcCm;
				//sensor.Mode = UltrasonicSensorMode.UsDcInch;
				sensor.Mode = UltrasonicSensorMode.Listen;
				while ( true )
				{
					//Console.Write( $"{sensor.Distance}      " );
					Console.Write( $"{sensor.Presence}      " );
					Console.SetCursorPosition( 0, Console.CursorTop );
					Thread.Sleep( 100 );
				}
			}
		}
	}
}
