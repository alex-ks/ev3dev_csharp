using System;

namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
	public enum GyroSensorMode
	{
		Angle,
		RotationSpeed,
		AngleAndRotationSpeed,
		GyroCal,
		GyroFas
	}

	public class GyroSensor : Sensor
	{
		public GyroSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

		/// <summary>
		/// Get or sets the mode of sensor.
		/// The sensor is calibrated when the <see cref="GyroSensorMode.RotationSpeed"/> 
		/// or the <see cref="GyroSensorMode.AngleAndRotationSpeed"/> mode is set.
		/// If the sensor is moving when setting the mode, the calibration will be off.
		/// </summary>
		public new GyroSensorMode Mode
		{
			get { return StringToMode( base.Mode ); }
			set { base.Mode = ModeToString( value ); }
		}

		/// <summary>
		/// Returns the current angle in degrees (from -32768 to 32767). 
		/// Clockwise is positive when looking at the side of the sensor with the arrows.
		/// The angle in <see cref="GyroSensorMode.Angle"/> or <see cref="GyroSensorMode.AngleAndRotationSpeed"/> 
		/// modes can be reset by changing to a different mode and changing back.
		/// NOTE: If you spin around too many times in <see cref="GyroSensorMode.Angle"/> 
		/// or <see cref="GyroSensorMode.AngleAndRotationSpeed"/> mode, it will get stuck at max value.
		/// </summary>
		public int Angle => GetValue( );

		/// <summary>
		/// Returns the current rotation speed. Clockwise is positive when 
		/// looking at the side of the sensor with the arrows.
		/// </summary>
		public int RotationSpeed => base.Mode == GyroRate ? GetValue( ) : GetValue( 1 );

		private GyroSensorMode StringToMode( string mode )
		{
			mode = mode.Trim( );
			switch ( mode )
			{
				case GyroAng:
					return GyroSensorMode.Angle;
				case GyroRate:
					return GyroSensorMode.RotationSpeed;
				case GyroFas:
					return GyroSensorMode.GyroFas;
				case GyroGAndA:
					return GyroSensorMode.AngleAndRotationSpeed;
				case GyroCal:
					return GyroSensorMode.GyroCal;
				default:
					throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private string ModeToString( GyroSensorMode mode )
		{
			switch ( mode )
			{
			case GyroSensorMode.Angle:
				return GyroAng;
			case GyroSensorMode.RotationSpeed:
				return GyroRate;
			case GyroSensorMode.AngleAndRotationSpeed:
				return GyroGAndA;
			case GyroSensorMode.GyroCal:
				return GyroCal;
			case GyroSensorMode.GyroFas:
				return GyroFas;
			default:
				throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		public const string GyroAng = "GYRO-ANG";
		public const string GyroRate = "GYRO-RATE";
		public const string GyroFas = "GYRO-FAS";
		public const string GyroGAndA = "GYRO-G&A";
		public const string GyroCal = "GYRO-CAL";

		public const string GyroSensorDriver = "lego-ev3-gyro";
		private static readonly string[] SuitableTypes = {GyroSensorDriver};
	}
}
