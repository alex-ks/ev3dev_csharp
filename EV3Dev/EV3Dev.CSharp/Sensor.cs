using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ev3Dev.CSharp
{
	public class Sensor : Device
	{
		private volatile byte[] _binaryData;

		/// <summary>
		/// Returns the name of the port that the sensor is connected to, e.g. `ev3:in1`. 
		/// I2C sensors also include the I2C address (decimal), e.g. `ev3:in1:i2c8`.
		/// </summary>
		public string Address => GetStringAttribute( AddressAttribute );

		/// <summary>
		/// Sends a command to the sensor.
		/// </summary>
		public string Command { set { SetStringAttribute( CommandAttribute, value ); } }

		/// <summary>
		/// Returns a list of the valid commands for the sensor. Returns -EOPNOTSUPP if no commands are supported.
		/// </summary>
		public string[] Commands => GetStringArrayAttribute( CommandsAttribute );

		/// <summary>
		/// Returns the number of decimal places for the values in the value&lt;N&gt; attributes of the current mode.
		/// </summary>
		public int Decimals => GetIntAttribute( DecimalsAttribute );

		/// <summary>
		/// Returns the name of the sensor device/driver. See the list of [supported sensors] for a complete list of drivers.
		/// </summary>
		public string DriverName => GetStringAttribute( DriverNameAttribute );

		/// <summary>
		/// Returns the current mode. Writing one of the values returned by modes sets the sensor to that mode.
		/// </summary>
		public string Mode
		{
			get { return GetStringAttribute( ModeAttribute ); }
			set { SetStringAttribute( ModeAttribute, value ); }
		}

		/// <summary>
		/// Returns a list of the valid modes for the sensor.
		/// </summary>
		public string[] Modes => GetStringArrayAttribute( ModesAttribute );

		/// <summary>
		/// Returns the number of value&lt;N&gt; attributes that will return a valid value for the current mode.
		/// </summary>
		public int NumValues => GetIntAttribute( NumValuesAttribute );

		/// <summary>
		/// Returns the units of the measured value for the current mode. May return empty string.
		/// </summary>
		public string Units => GetStringAttribute( UnitsAttribute );

		public const string AddressAttribute = "address";
		public const string CommandAttribute = "command";
		public const string CommandsAttribute = "commands";
		public const string DecimalsAttribute = "decimals";
		public const string DriverNameAttribute = "driver_name";
		public const string ModeAttribute = "mode";
		public const string ModesAttribute = "modes";
		public const string NumValuesAttribute = "num_values";
		public const string UnitsAttribute = "units";
	}
}
