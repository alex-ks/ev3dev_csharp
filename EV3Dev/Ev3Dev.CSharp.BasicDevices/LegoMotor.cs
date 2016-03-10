using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EV3Dev.CSharp;

namespace Ev3Dev.CSharp.BasicDevices
{
	public enum Polarity
	{
		Normal, Inversed
	}

	public enum StopCommand
	{
		/// <summary>
		/// Power will be removed from the motor and it will freely coast to a stop.
		/// </summary>
		Coast,
		/// <summary>
		/// Power will be removed from the motor and a passive electrical load will be placed on the motor. 
		/// This is usually done by shorting the motor terminals together. 
		/// This load will absorb the energy from the rotation of the motors and cause the motor to stop more quickly than coasting. 
		/// </summary>
		Brake,
		/// <summary>
		/// Does not remove power from the motor. Instead it actively try to hold the motor at the current position. 
		/// If an external force tries to turn the motor, the motor will ‘push back’ to maintain its position.
		/// </summary>
		Hold
	}

	public class LegoMotor : Motor
	{
		private static readonly IDictionary<OutputPort, string> PortNames;

		static LegoMotor( )
		{
			PortNames = new Dictionary<OutputPort, string>
			{
				{ OutputPort.OutA, "outA" },
				{ OutputPort.OutB, "outB" },
				{ OutputPort.OutC, "outC" },
				{ OutputPort.OutD, "outD" }
			};
		}

		protected LegoMotor( OutputPort port, string motorType ) : base( PortNames[port], motorType )
		{

		}

		public new Polarity Polarity
		{
			get { return base.Polarity == PolarityNormal ? Polarity.Normal : Polarity.Inversed; }
			set { base.Polarity = value == Polarity.Normal ? PolarityNormal : PolarityInversed; }
		}

		public new bool SpeedRegulationEnabled
		{
			get { return base.SpeedRegulationEnabled == SpeedRegulationOn; }
			set { base.SpeedRegulationEnabled = value ? SpeedRegulationOn : SpeedRegulationOff; }
		}

		public new StopCommand StopCommand
		{
			get { return StringToStopCommand( base.StopCommand ); }
			set { base.StopCommand = StopCommandToString( value ); }
		}

		public void RunForever( )
		{
			Command = CommandRunForever;
		}

		public void RunForever( int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Command = CommandRunForever;
			DutyCycleSp = old;
		}

		public void RunTimed( int ms )
		{
			TimeSp = ms;
			Command = CommandRunTimed;
		}

		public void RunTimed( int ms, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			RunTimed( ms );
			DutyCycleSp = old;
		}

		public void Stop( )
		{
			Command = CommandStop;
		}

		public void Stop( StopCommand command )
		{
			var old = StopCommand;
			StopCommand = command;
			Stop( );
			StopCommand = old;
		}

		public void Reset( )
		{
			Command = CommandReset;
		}

		public void Run( int degrees )
		{
			PositionSp = ( int )( degrees * ( CountPerRot / 360.0 ) );
			Command = CommandRunToRelPos;
		}

		public void Run( int degrees, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Run( degrees );
			DutyCycleSp = old;
		}

		public void Run( float rotations )
		{
			PositionSp = ( int )( rotations * CountPerRot );
			Command = CommandRunToRelPos;
		}

		public void Run( float rotations, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Run( rotations );
			DutyCycleSp = old;
		}

		public void RunDirect( )
		{
			Command = CommandRunDirect;
		}

		public void WaitForStop( int waitTime = 50 )
		{
			while ( DutyCycle != 0 )
			{
				Thread.Sleep( waitTime );
			}
		}

		private static StopCommand StringToStopCommand( string command )
		{
			switch ( command )
			{
				case StopCommandCoast:
					return StopCommand.Coast;
				case StopCommandBrake:
					return StopCommand.Brake;
				case StopCommandHold:
					return StopCommand.Hold;
				default:
					return StopCommand.Coast;
			}
		}

		private static string StopCommandToString( StopCommand command )
		{
			switch ( command )
			{
				case StopCommand.Coast:
					return StopCommandCoast;
				case StopCommand.Brake:
					return StopCommandBrake;
				case StopCommand.Hold:
					return StopCommandHold;
				default:
					return StopCommandCoast;
			}
		}
	}
}
