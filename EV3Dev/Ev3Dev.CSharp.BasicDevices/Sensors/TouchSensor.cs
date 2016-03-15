namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
	public enum TouchSensorState
	{
		Released = 0,
		Pressed = 1
	}

	public class TouchSensor : Sensor
	{
		public TouchSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

		public TouchSensorState State => ( TouchSensorState )GetValue( );

		private static readonly string[] SuitableTypes = { TouchSensorDriver };
		private const string TouchSensorDriver = "lego-ev3-touch";
	}
}
