using System;

namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
    /// <summary>
    /// See http://docs.ev3dev.org/projects/lego-linux-drivers/en/ev3dev-jessie/sensor_data.html#lego-ev3-ir for details.
    /// </summary>
	public enum InfraredSensorMode
	{
		Proximity,
		IrSeeker,
        /// <summary>
        /// Remote control mode similar to default EV3 remote control mode.
        /// </summary>
		IrRemoteControl,
        /// <summary>
        /// Use <see cref="InfraredRemoteDecoder"/> extensions to decode this mode values.
        /// See http://docs.ev3dev.org/projects/lego-linux-drivers/en/ev3dev-jessie/sensor_data.html#lego-ev3-ir-mode3-value0
        /// for details.
        /// </summary>
		IrRemoteControlAlternative,
		/// <summary>
		/// This mode is not usable. When switching to this mode, 
		/// the sensor quits responding to the keep-alive messages and the sensor resets.
		/// </summary>
		IrSAlt,
		IrCal
	}

	public enum RemoteControlButton
	{
		None = 0,
		RedUp = 1,
		RedDown = 2,
		BlueUp = 3,
		BlueDown = 4,
		RedUpAndBlueUp = 5,
		RedUpAndBlueDown = 6,
		RedDownAndBlueUp = 7,
		RedDownAndBlueDown = 8,
		BeaconModeOn = 9,
		RedUpAndRedDown = 10,
		BlueUpAndBlueDown = 11
	}

	public class InfraredSensor : Sensor
	{
		private const int ButtonEnumMax = 11;

		public InfraredSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

        /// <summary>
        /// It seems that IR mode change takes some time, so if we read its value right after the mode switch 
        /// we may get some garbage instead of real values. So it's better to wait some time (my tests show 
        /// that 30-40 ms is OK) before reading values.
        /// </summary>
		public new InfraredSensorMode Mode
		{
			get { return StringToMode( base.Mode ); }
			set { base.Mode = ModeToString( value ); }
		}

		/// <summary>
		/// If mode is <see cref="InfraredSensorMode"/>.Proximity, returns proximity in percents (100% is approximately 70cm/27in).
		/// Otherwise, returns -128.
		/// </summary>
		public int Proximity => Mode == InfraredSensorMode.Proximity ? GetValue( ) : -128;

		/// <summary>
		/// If mode is <see cref="InfraredSensorMode"/>.IrSeeker, returns distance to beacon
		/// in percents (100% is approximately 70cm/27in). 
		/// If beacon is not found or mode is not <see cref="InfraredSensorMode"/>.IrSeeker, returns -128.
		/// </summary>
		/// <param name="channel">Channel of beacon broadcasting (1-4).</param>
		public int GetDistanceToBeacon( int channel )
		{
			return Mode == InfraredSensorMode.IrSeeker ? GetValue( channel * 2 - 1 ) : -128;
		}

		/// <summary>
		/// If mode is <see cref="InfraredSensorMode"/>.IrSeeker, returns heading to beacon from -25 (far left) to 25 (far right).
		/// If beacon is not found or mode is not <see cref="InfraredSensorMode"/>.IrSeeker, returns 0.
		/// </summary>
		/// <param name="channel">Channel of beacon broadcasting (1-4).</param>
		/// <returns></returns>
		public int GetHeadingToBeacon( int channel )
		{
			return Mode == InfraredSensorMode.IrSeeker ? GetValue( channel * 2 - 2 ) : 0;
		}

		/// <summary>
		/// If mode is <see cref="InfraredSensorMode"/>.IrRemoteControl, returns button combination for the specified channel.
		/// Otherwise, returns <see cref="RemoteControlButton"/>.None.
		/// </summary>
		/// <param name="channel">Channel of remote control signal (1-4).</param>
		public RemoteControlButton GetPressedButton( int channel )
		{
			if ( Mode != InfraredSensorMode.IrRemoteControl )
			{ return RemoteControlButton.None; }
			var value = GetValue( channel - 1 );
			if ( 0 <= value && value <= ButtonEnumMax )
			{ return ( RemoteControlButton )value; }
			else
			{ return RemoteControlButton.None; }
		}

		private InfraredSensorMode StringToMode( string mode )
		{
			mode = mode.Trim( );
			switch ( mode )
			{
				case IrProx:
					return InfraredSensorMode.Proximity;
				case IrSeek:
					return InfraredSensorMode.IrSeeker;
				case IrRemote:
					return InfraredSensorMode.IrRemoteControl;
				case IrRemA:
					return InfraredSensorMode.IrRemoteControlAlternative;
				case IrSAlt:
					return InfraredSensorMode.IrSAlt;
				case IrCal:
					return InfraredSensorMode.IrCal;
				default:
					throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		private string ModeToString( InfraredSensorMode mode )
		{
			switch ( mode )
			{
			case InfraredSensorMode.Proximity:
				return IrProx;
			case InfraredSensorMode.IrSeeker:
				return IrSeek;
			case InfraredSensorMode.IrRemoteControl:
				return IrRemote;
			case InfraredSensorMode.IrRemoteControlAlternative:
				return IrRemA;
			case InfraredSensorMode.IrSAlt:
				return IrSAlt;
			case InfraredSensorMode.IrCal:
				return IrCal;
			default:
				throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
			}
		}

		public const string InfraredSensorDriver = "lego-ev3-ir";
		private static readonly string[] SuitableTypes = {InfraredSensorDriver};

		private const string IrProx = "IR-PROX";
		private const string IrSeek = "IR-SEEK";
		private const string IrRemote = "IR-REMOTE";
		private const string IrRemA = "IR-REM-A";
		private const string IrSAlt = "IR-S-ALT";
		private const string IrCal = "IR-CAL";
	}
}
