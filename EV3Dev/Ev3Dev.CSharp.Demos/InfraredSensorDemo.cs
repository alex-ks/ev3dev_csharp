using System;
using System.Text;
using System.Threading;
using Ev3Dev.CSharp.BasicDevices;

namespace Ev3Dev.CSharp.Demos
{
	public class InfraredSensorDemo
	{
		/// <summary>
		/// Writes whitespaces to console to remove symbols from last print
		/// </summary>
		/// <param name="previousEndPos">Position of cursor after writing previous value</param>
		/// <returns></returns>
		private static int EraseOld( int previousEndPos )
		{
			StringBuilder builder = new StringBuilder( );
			int length = Console.CursorLeft;

			while ( length + builder.Length < previousEndPos )
			{
				builder.Append( ' ' );
			}

			if ( builder.Length != 0 )
			{ Console.Write( builder.ToString( ) );}

			Console.WriteLine( );
			return length;
		}

		public static void Main( string[] args )
		{
			Console.WriteLine( "Connecting to sensor..." );
			using ( var sensor = new InfraredSensor( InputPort.In2 ) )
			{
				Console.WriteLine( "Proximity:" );
				var proxPos = new { Left = Console.CursorLeft, Top = Console.CursorTop };
				sensor.Mode = InfraredSensorMode.Proximity;
				Console.WriteLine( sensor.Proximity );

				Console.WriteLine( "Beacon:" );
				var beaconPos = new { Left = Console.CursorLeft, Top = Console.CursorTop };
				sensor.Mode = InfraredSensorMode.IrSeeker;
				Console.WriteLine( $"Heading = {sensor.GetHeadingToBeacon( 1 )}" );
				Console.WriteLine( $"Distance = {sensor.GetDistanceToBeacon( 1 )}" );

				Console.Write( "Button: " );
				var buttonPos = new { Left = Console.CursorLeft, Top = Console.CursorTop };
				sensor.Mode = InfraredSensorMode.IrRemoteControl;
				Console.WriteLine( sensor.GetPressedButton( 1 ).ToString( ) );

				Console.CursorVisible = false;

				int
					proxLastLength = 0,
					headLastLength = 0,
					distLastLength = 0,
					buttonLastLength = 0;

				while ( true )
				{
					Console.SetCursorPosition( proxPos.Left, proxPos.Top );
					sensor.Mode = InfraredSensorMode.Proximity;
					// If we read value immediately after mode change, we may read read garbage
					Thread.Sleep( 33 );
					Console.Write( $"{sensor.Proximity}" );
					proxLastLength = EraseOld( proxLastLength );

					// Due to some reason, seeker mode does not provide correct values if it is set
					// before each measurement. So this print is just for code demonstration
					Console.SetCursorPosition( beaconPos.Left, beaconPos.Top );
					sensor.Mode = InfraredSensorMode.IrSeeker;
					Thread.Sleep( 33 );
					Console.Write( $"Heading = {sensor.GetHeadingToBeacon( 1 )}" );
					headLastLength = EraseOld( headLastLength );
					Console.Write( $"Distance = {sensor.GetDistanceToBeacon( 1 )}   " );
					distLastLength = EraseOld( distLastLength );

					Console.SetCursorPosition( buttonPos.Left, buttonPos.Top );
					sensor.Mode = InfraredSensorMode.IrRemoteControl;
					Thread.Sleep( 33 );
					Console.Write( sensor.GetPressedButton( 1 ) );
					buttonLastLength = EraseOld( buttonLastLength );
				}
			}
		}
	}
}
