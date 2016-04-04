using System;

namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
	public enum UltrasonicSensorMode
	{
		ContinuousMeasurementCm,
		ContinuousMeasurementInch,
		Listen,
		SingleMeasurementCm,
		SingleMeasurementInch,
		UsDcCm,
		UsDcInch
	}

	public class UltrasonicSensor : Sensor
	{
		public UltrasonicSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

		/// <summary>
		/// Returns distance in centimeters or inchers (depends on mode).
		/// If the mode is <see cref="UltrasonicSensorMode.Listen"/>, returns <see cref="double.NaN"/>.
		/// If the mode is <see cref="UltrasonicSensorMode.SingleMeasurementCm"/> or 
		/// <see cref="UltrasonicSensorMode.SingleMeasurementInch"/>, 
		/// the value won't change until the next mode setting.
		/// </summary>
		public double Distance => base.Mode != UsListen ? GetValue( ) / 10.0 : double.NaN;

		/// <summary>
		/// Indicates whether another ultrasonic sensor has been detected.
		/// Also can be triggered by a loud noise such as clapping.
		/// If mode is not <see cref="UltrasonicSensorMode.Listen"/> returns false.
		/// </summary>
		public bool Presence => base.Mode == UsListen && GetValue( ) == 1;

		/// <summary>
		/// Gets or sets current sensor mode. 
		/// NOTE: If you write the mode too frequently (e.g. every 100 ms), 
		/// the sensor will sometimes lock up and writing to the mode attribute will return an error. 
		/// A delay of 250 ms between each write to the mode attribute seems sufficient 
		/// to keep the sensor from locking up.
		/// </summary>
		public new UltrasonicSensorMode Mode
		{
			get { return StringToMode( base.Mode ); }
			set { base.Mode = ModeToString( value ); }
		}

		private UltrasonicSensorMode StringToMode( string mode )
		{
			mode = mode.Trim( );
			switch ( mode )
			{
				case UsDistCm:
					return UltrasonicSensorMode.ContinuousMeasurementCm;
				case UsDistIn:
					return UltrasonicSensorMode.ContinuousMeasurementInch;
				case UsListen:
					return UltrasonicSensorMode.Listen;
				case UsSiCm:
					return UltrasonicSensorMode.SingleMeasurementCm;
				case UsSiIn:
					return UltrasonicSensorMode.SingleMeasurementInch;
				case UsDcCm:
					return UltrasonicSensorMode.UsDcCm;
				case UsDcIn:
					return UltrasonicSensorMode.UsDcInch;
				default:
					throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private string ModeToString( UltrasonicSensorMode mode )
		{
			switch ( mode )
			{
			case UltrasonicSensorMode.ContinuousMeasurementCm:
				return UsDistCm;
			case UltrasonicSensorMode.ContinuousMeasurementInch:
				return UsDistIn;
			case UltrasonicSensorMode.Listen:
				return UsListen;
			case UltrasonicSensorMode.SingleMeasurementCm:
				return UsSiCm;
			case UltrasonicSensorMode.SingleMeasurementInch:
				return UsSiIn;
			case UltrasonicSensorMode.UsDcCm:
				return UsDcCm;
			case UltrasonicSensorMode.UsDcInch:
				return UsDcIn;
			default:
				throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private const string UsDistCm = "US-DIST-CM";
		private const string UsDistIn = "US-DIST-IN";
		private const string UsListen = "US-LISTEN";
		private const string UsSiCm = "US-SI-CM";
		private const string UsSiIn = "US-SI-IN";
		private const string UsDcCm = "US-DC-CM";
		private const string UsDcIn = "US-DC-IN";

		public const string UltrasonicSensorDriver = "lego-ev3-us";
		private static readonly string[] SuitableTypes = {UltrasonicSensorDriver};
	}
}
