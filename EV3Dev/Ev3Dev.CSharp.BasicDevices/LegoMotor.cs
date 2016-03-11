using System.Collections.Generic;
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

		/// <summary>
		/// Period of motor polling when waiting for motor stop in milliseconds.
		/// </summary>
		public int PollPeriod { get; set; } = 50;

		public new Polarity Polarity
		{
			get { return base.Polarity == MotorCommands.PolarityNormal ? Polarity.Normal : Polarity.Inversed; }
			set { base.Polarity = value == Polarity.Normal ? MotorCommands.PolarityNormal : MotorCommands.PolarityInversed; }
		}

		public new bool SpeedRegulationEnabled
		{
			get { return base.SpeedRegulationEnabled == MotorCommands.SpeedRegulationOn; }
			set { base.SpeedRegulationEnabled = value ? MotorCommands.SpeedRegulationOn : MotorCommands.SpeedRegulationOff; }
		}

		public new StopCommand StopCommand
		{
			get { return StringToStopCommand( base.StopCommand ); }
			set { base.StopCommand = StopCommandToString( value ); }
		}

		public void RunForever( )
		{
			Command = MotorCommands.CommandRunForever;
		}

		public void RunForever( int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Command = MotorCommands.CommandRunForever;
			DutyCycleSp = old;
		}

		public LazyTask RunTimed( int ms )
		{
			TimeSp = ms;
			Command = MotorCommands.CommandRunTimed;
			return new LazyTask( WaitForStop );
		}

		public LazyTask RunTimed( int ms, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			RunTimed( ms );
			DutyCycleSp = old;
			return new LazyTask( WaitForStop );
		}

		public void Stop( )
		{
			Command = MotorCommands.CommandStop;
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
			Command = MotorCommands.CommandReset;
		}

		public LazyTask Run( int degrees )
		{
			PositionSp = ( int )( degrees * ( CountPerRot / 360.0 ) );
			Command = MotorCommands.CommandRunToRelPos;
			return new LazyTask( WaitForStop );
		}

		public LazyTask Run( int degrees, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Run( degrees );
			DutyCycleSp = old;
			return new LazyTask( WaitForStop );
		}

		public LazyTask Run( float rotations )
		{
			PositionSp = ( int )( rotations * CountPerRot );
			Command = MotorCommands.CommandRunToRelPos;
			return new LazyTask( WaitForStop );
		}

		public LazyTask Run( float rotations, int speed )
		{
			var old = DutyCycleSp;
			DutyCycleSp = speed;
			Run( rotations );
			DutyCycleSp = old;
			return new LazyTask( WaitForStop );
		}

		public void RunDirect( )
		{
			Command = MotorCommands.CommandRunDirect;
		}

		public void WaitForStop( )
		{
			while ( DutyCycle != 0 )
			{
				Thread.Sleep( PollPeriod );
			}
		}

		private static StopCommand StringToStopCommand( string command )
		{
			switch ( command )
			{
				case MotorCommands.StopCommandCoast:
					return StopCommand.Coast;
				case MotorCommands.StopCommandBrake:
					return StopCommand.Brake;
				case MotorCommands.StopCommandHold:
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
					return MotorCommands.StopCommandCoast;
				case StopCommand.Brake:
					return MotorCommands.StopCommandBrake;
				case StopCommand.Hold:
					return MotorCommands.StopCommandHold;
				default:
					return MotorCommands.StopCommandCoast;
			}
		}
	}
}
