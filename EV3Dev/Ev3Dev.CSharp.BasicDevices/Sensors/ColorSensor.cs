using System;

namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
	public enum ColorSensorMode
	{
		/// <summary>
		/// Detects reflected light intensity in percents.
		/// </summary>
		ReflectedLight,
		/// <summary>
		/// Detects ambient light intensity in percents.
		/// </summary>
		AmbientLight,
		/// <summary>
		/// Detects one of seven possible values.
		/// </summary>
		Color,
		/// <summary>
		/// Two value components, purpose is unknown (values range is 0..1020(?)).
		/// </summary>
		RawReflected,
		/// <summary>
		/// Three value components, purpose is unknown (possibly RGB components, but range is 0..1020(?)).
		/// </summary>
		RawColorComponents,
		/// <summary>
		/// This mode is not usable. When in COL-CAL mode, 
		/// the color sensor does not respond to the keep-alive sent from the EV3 brick. 
		/// As a result, the sensor will time out and reset.
		/// </summary>
		Calibration
	}

	public class ColorSensor : Sensor
	{
		private const int ColorMax = 7;

		public enum DetectedColor
		{
			None = 0,
			Black = 1,
			Blue = 2,
			Green = 3,
			Yellow = 4,
			Red = 5,
			White = 6,
			Brown = 7
		}

		public ColorSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

		public new ColorSensorMode Mode
		{
			get { return StringToMode( base.Mode ); }
			set { base.Mode = ModeToString( value ); }
		}

		/// <summary>
		/// If sensor mode is <see cref="ColorSensorMode"/>.ReflectedLight or <see cref="ColorSensorMode"/>.AmbientLight,
		/// returns light intensity in percents. Otherwise, returns -1.
		/// </summary>
		public int LightIntensity
		{
			get
			{
				var mode = Mode;
				if ( mode == ColorSensorMode.AmbientLight || mode == ColorSensorMode.ReflectedLight )
				{ return GetValue( ); }
				else
				{ return -1; }
			}
		}

		/// <summary>
		/// If sensor mode is <see cref="ColorSensorMode"/>.Color, returns detected color.
		/// Otherwise, returns <see cref="ColorSensorMode"/>.None.
		/// </summary>
		public DetectedColor Color
		{
			get
			{
				if ( Mode != ColorSensorMode.Color )
				{ return DetectedColor.None; }
				var value = GetValue( );
				if ( 0 <= value && value <= ColorMax )
				{ return ( DetectedColor )value; }
				else
				{ return DetectedColor.None; }
			}
		}

		private ColorSensorMode StringToMode( string mode )
		{
			switch ( mode.Trim( ) )
			{
				case ColReflect:
					return ColorSensorMode.ReflectedLight;
				case ColAmbient:
					return ColorSensorMode.AmbientLight;
				case ColColor:
					return ColorSensorMode.Color;
				case RefRaw:
					return ColorSensorMode.RawReflected;
				case RgbRaw:
					return ColorSensorMode.RawColorComponents;
				case ColCal:
					return ColorSensorMode.Calibration;
				default:
					throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private string ModeToString( ColorSensorMode mode )
		{
			switch ( mode )
			{
			case ColorSensorMode.ReflectedLight:
				return ColReflect;
			case ColorSensorMode.AmbientLight:
				return ColAmbient;
			case ColorSensorMode.Color:
				return ColColor;
			case ColorSensorMode.RawReflected:
				return RefRaw;
			case ColorSensorMode.RawColorComponents:
				return RgbRaw;
			case ColorSensorMode.Calibration:
				return ColCal;
			default:
				throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private static readonly string[] SuitableTypes = {ColorSensorDriver};
		public const string ColorSensorDriver = "lego-ev3-color";

		private const string ColReflect = "COL-REFLECT";
		private const string ColAmbient = "COL-AMBIENT";
		private const string ColColor = "COL-COLOR";
		private const string RefRaw = "REF-RAW";
		private const string RgbRaw = "RGB-RAW";
		private const string ColCal = "COL-CAL";
	}
}
