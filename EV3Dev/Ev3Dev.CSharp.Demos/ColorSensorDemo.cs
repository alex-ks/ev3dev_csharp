﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ev3Dev.CSharp.BasicDevices;
using Ev3Dev.CSharp.BasicDevices.Sensors;

namespace Ev3Dev.CSharp.Demos
{
	public class ColorSensorDemo
	{
		public static void Main( string[] args )
		{
			Console.WriteLine( "Connecting to sensor..." );
			using ( var sensor = new ColorSensor( InputPort.In2 ) )
			{
				sensor.Mode = ColorSensorMode.Color;
				while ( true )
				{
					Console.Write( $"{sensor.Color}      " );
					Console.SetCursorPosition( 0, Console.CursorTop );
					Thread.Sleep( 100 );
				}
			}
		}
	}
}
