using System;

namespace Ev3Dev.CSharp.BasicDevices
{
	public enum InfraredSensorMode
	{
		Proximity,
		IrSeeker,
		IrRemoteControl,
		IrRemoteControlAlternative,
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
		public InfraredSensor( InputPort port ) : base( port.ToStringName( ), SuitableTypes )
		{
			
		}

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
			return Mode == InfraredSensorMode.IrRemoteControl
				? ( RemoteControlButton )GetValue( channel - 1 )
				: RemoteControlButton.None;
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
