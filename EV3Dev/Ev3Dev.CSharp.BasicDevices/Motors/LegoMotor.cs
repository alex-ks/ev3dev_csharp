using System;
using System.Threading;
using EV3Dev.CSharp;

namespace Ev3Dev.CSharp.BasicDevices.Motors
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
		protected LegoMotor( OutputPort port, string motorType ) : base( port.ToStringName( ), motorType )
		{

		}

		/// <summary>
		/// Period of motor polling when waiting for motor stop in milliseconds.
		/// </summary>
		public int PollPeriod { get; set; } = 50;

		/// <summary>
		/// Sets the polarity of the motor. 
		/// With normal polarity, a positive duty cycle will cause the motor to rotate clockwise. 
		/// With inversed polarity, a positive duty cycle will cause the motor to rotate counter-clockwise.
		/// </summary>
		public new Polarity Polarity
		{
			get { return base.Polarity == MotorCommands.PolarityNormal ? Polarity.Normal : Polarity.Inversed; }
			set { base.Polarity = value == Polarity.Normal ? MotorCommands.PolarityNormal : MotorCommands.PolarityInversed; }
		}

		/// <summary>
		/// Turns speed regulation on or off. If speed regulation is on, 
		/// the motor controller will vary the power supplied to the motor to try to maintain the speed specified in <see cref="Motor.SpeedSp"/>. 
		/// If speed regulation is off, the controller will use the power specified in <see cref="Motor.DutyCycleSp"/>.
		/// </summary>
		public new bool SpeedRegulationEnabled
		{
			get { return base.SpeedRegulationEnabled == MotorCommands.SpeedRegulationOn; }
			set { base.SpeedRegulationEnabled = value ? MotorCommands.SpeedRegulationOn : MotorCommands.SpeedRegulationOff; }
		}

		/// <summary>
		/// The value determines the motors behavior when command <see cref="Stop()"/> is called. 
		/// Also, it determines the motors behavior when a run command completes.
		/// </summary>
		public new StopCommand StopCommand
		{
			get { return StringToStopCommand( base.StopCommand ); }
			set { base.StopCommand = StopCommandToString( value ); }
		}

		/// <summary>
		/// Run the motor until another command is called.
		/// </summary>
		public void RunForever( )
		{
			Command = MotorCommands.CommandRunForever;
		}

		/// <summary>
		/// Run the motor with specified power until another command is called.
		/// </summary>
		/// <param name="power">Motor power in percents (from -100 to 100).</param>
		/// <remarks>
		/// This method reads old values of <see cref="Motor.DutyCycleSp"/> and <see cref="Motor.SpeedRegulationEnabled"/>, and if they are different from arguments
		///	sets <see cref="Motor.SpeedRegulationEnabled"/> to <see cref="MotorCommands.SpeedRegulationOff"/>, <see cref="Motor.DutyCycleSp"/>
		/// to power and then restores the old values.
		/// </remarks>
		public void RunForever( sbyte power )
		{
			var speedReg = SpeedRegulationEnabled;
			var old = DutyCycleSp;

			try
			{
				if ( old != power )
				{ DutyCycleSp = power; }
				if ( speedReg )
				{ SpeedRegulationEnabled = false; }

				Command = MotorCommands.CommandRunForever;
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != power )
					{ DutyCycleSp = old; }
					if ( speedReg )
					{ SpeedRegulationEnabled = speedReg; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Run the motor with specified speed until another command is called.
		/// </summary>
		/// <param name="speed">Motor speed in degrees per second.</param>
		public void RunForever( int speed )
		{
			var speedReg = SpeedRegulationEnabled;
			var old = SpeedSp;
			var tachoSpeed = ( int )( speed * ( CountPerRot / 360.0 ) );

			try
			{
				if ( old != tachoSpeed )
				{ SpeedSp = tachoSpeed; }
				if ( !speedReg )
				{ SpeedRegulationEnabled = true; }

				Command = MotorCommands.CommandRunForever;
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( tachoSpeed != old )
					{ SpeedSp = old; }
					if ( !speedReg )
					{ SpeedRegulationEnabled = speedReg; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Run the motor for the specified amount of time
		/// and then stop the motor using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="ms">Time in milleseconds.</param>
		/// <returns>Task for execution finish waiting.</returns>
		public LazyTask RunTimed( int ms )
		{
			TimeSp = ms;
			var start = DateTime.Now;
			Command = MotorCommands.CommandRunTimed;
			return new LazyTask( ( ) => WaitForPeriod( start, ms ) );
		}

		/// <summary>
		/// Run the motor for the specified amount of time with the specified power
		/// and then stop the motor using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="ms">Time in milleseconds.</param>
		/// <param name="power">Motor power in percents (from -100 to 100).</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask RunTimed( int ms, sbyte power )
		{
			var old = DutyCycleSp;
			var speedReg = SpeedRegulationEnabled;

			try
			{
				if ( old != power )
				{ DutyCycleSp = power; }
				if ( speedReg )
				{ SpeedRegulationEnabled = false; }

				var start = DateTime.Now;
				RunTimed( ms );

				return new LazyTask( ( ) => WaitForPeriod( start, ms ) );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != power )
					{ DutyCycleSp = old; }
					if ( speedReg )
					{ SpeedRegulationEnabled = true; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Run the motor for the specified amount of time with the specified speed
		/// and then stop the motor using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="ms">Time in milleseconds.</param>
		/// <param name="speed">Motor speed in degrees per second.</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask RunTimed( int ms, int speed )
		{
			var old = SpeedSp;
			var speedReg = SpeedRegulationEnabled;
			var tachoSpeed = ( int )( speed * ( CountPerRot / 360.0 ) );

			try
			{
				if ( old != tachoSpeed )
				{ SpeedSp = tachoSpeed; }
				if ( !speedReg )
				{ SpeedRegulationEnabled = true; }

				var start = DateTime.Now;
				RunTimed( ms );

				return new LazyTask( ( ) => WaitForPeriod( start, ms ) );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != tachoSpeed )
					{ SpeedSp = old; }
					if ( !speedReg )
					{ SpeedRegulationEnabled = false; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Stop any of the run commands before they are complete using the
		/// command specified by <see cref="StopCommand"/> property.
		/// </summary>
		public void Stop( )
		{
			Command = MotorCommands.CommandStop;
		}

		/// <summary>
		/// Stop any of the run commands before they are complete using the specified stop command.
		/// </summary>
		/// <param name="command">Describes how to stop the motor.</param>
		public void Stop( StopCommand command )
		{
			var old = StopCommand;
			if ( command != old )
			{ StopCommand = command; }
			Stop( );
			if ( command != old )
			{ StopCommand = old; }
		}

		/// <summary>
		/// Reset all of the motor parameter attributes to their default value.
		/// This will also have the effect of stopping the motor.
		/// </summary>
		public void Reset( )
		{
			Command = MotorCommands.CommandReset;
		}

		/// <summary>
		/// Turn the motor on specified amount of degrees and then stop the motor
		/// using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="degrees">Amount of degrees to turn the motor.</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask Run( int degrees )
		{
			var setPoint = ( int )( degrees * ( CountPerRot / 360.0 ) );
			PositionSp = setPoint;
			var finish = Position + setPoint;
			Command = MotorCommands.CommandRunToRelPos;
			return new LazyTask( ( ) => WaitForPosition( finish, degrees > 0 ) );
		}

		/// <summary>
		/// Turn the motor on specified amount of degrees  with the specified power and then stop the motor
		/// using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="degrees">Amount of degrees to turn the motor.</param>
		/// <param name="power">Motor power in percents (from -100 to 100).</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask Run( int degrees, sbyte power )
		{
			var old = DutyCycleSp;
			var speedReg = SpeedRegulationEnabled;

			try
			{
				if ( old != power )
				{ DutyCycleSp = power; }
				if ( speedReg )
				{ SpeedRegulationEnabled = false; }

				return Run( degrees );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != power )
					{ DutyCycleSp = old; }
					if ( speedReg )
					{ SpeedRegulationEnabled = true; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Turn the motor on specified amount of degrees  with the specified speed and then stop the motor
		/// using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="degrees">Amount of degrees to turn the motor.</param>
		/// <param name="speed">Motor speed in degrees per second.</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask Run( int degrees, int speed )
		{
			var old = SpeedSp;
			var speedReg = SpeedRegulationEnabled;
			var tachoSpeed = ( int )( speed * ( CountPerRot / 360.0 ) );

			try
			{
				if ( old != tachoSpeed )
				{ SpeedSp = tachoSpeed; }
				if ( !speedReg )
				{ SpeedRegulationEnabled = true; }

				return Run( degrees );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != tachoSpeed )
					{ SpeedSp = old; }
					if ( !speedReg )
					{ SpeedRegulationEnabled = false; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Turn the motor on specified amount of rotations and then stop the motor
		/// using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="rotations">Amount of rotations to turn the motor.</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask Run( float rotations )
		{
			var setPoint = ( int )( rotations * CountPerRot );
			PositionSp = setPoint;
			var finish = Position + setPoint;
			Command = MotorCommands.CommandRunToRelPos;
			return new LazyTask( ( ) => WaitForPosition( finish, rotations > 0 ) );
		}

		/// <summary>
		/// Turn the motor on specified amount of rotations with the specified power
		/// and then stop the motor using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="rotations">Amount of rotations to turn the motor.</param>
		/// <param name="power">Motor power in percents (from -100 to 100).</param>
		/// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
		public LazyTask Run( float rotations, sbyte power )
		{
			var old = DutyCycleSp;
			var speedReg = SpeedRegulationEnabled;

			try
			{
				if ( old != power )
				{ DutyCycleSp = power; }
				if ( speedReg )
				{ SpeedRegulationEnabled = false; }

				return Run( rotations );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != power )
					{ DutyCycleSp = old; }
					if ( speedReg )
					{ SpeedRegulationEnabled = true; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Turn the motor on specified amount of rotations with the specified speed
		/// and then stop the motor using the command specified by <see cref="StopCommand"/> property.
		/// </summary>
		/// <param name="rotations"></param>
		/// <param name="speed"></param>
		/// <returns></returns>
		public LazyTask Run( float rotations, int speed )
		{
			var old = SpeedSp;
			var speedReg = SpeedRegulationEnabled;
			var tachoSpeed = ( int )( speed * ( CountPerRot / 360.0 ) );

			try
			{
				if ( old != tachoSpeed )
				{ SpeedSp = tachoSpeed; }
				if ( !speedReg )
				{ SpeedRegulationEnabled = true; }

				return Run( rotations );
			}
			finally
			{
				// to avoid double throw
				try
				{
					if ( old != tachoSpeed )
					{ SpeedSp = old; }
					if ( !speedReg )
					{ SpeedRegulationEnabled = false; }
				}
				catch ( Exception )
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Run the motor at the duty cycle specified by <see cref="Motor.Speed"/>.
		/// Unlike other run commands, changing <see cref="Motor.Speed"/> while running *will*
		/// take effect immediately.
		/// </summary>
		public void RunDirect( )
		{
			Command = MotorCommands.CommandRunDirect;
		}

		//todo: check DutySycleSp while "hold" stop command
		/// <summary>
		/// Wait until the motor speed is equal to 0.
		/// </summary>
		public void WaitForStop( )
		{
			while ( Speed != 0 )
			{
				Thread.Sleep( PollPeriod );
			}
		}

		public void WaitForPeriod( DateTime start, int ms )
		{
			var remain = ms - ( int )( DateTime.Now - start ).TotalMilliseconds;
			if ( remain > 0 )
			{ Thread.Sleep( remain ); }
		}

		/// <summary>
		/// Wait until motor tacho counter reaches specified position.
		/// </summary>
		/// <param name="position">
		/// Tacho counter position in pulses of rotary encoder. 
		/// See <see cref="Motor.CountPerRot"/> for conversion info.
		/// </param>
		/// <param name="clockwise">
		/// Direction of motor rotation.
		/// </param>
		public void WaitForPosition( int position, bool clockwise )
		{
			Func<int, bool> stopCriterion;
			if ( clockwise )
			{ stopCriterion = curr => curr >= position; }
			else
			{ stopCriterion = curr => curr <= position; }

			while ( stopCriterion( Position ) )
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
