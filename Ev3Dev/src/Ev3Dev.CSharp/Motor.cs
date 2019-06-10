using System;
using System.Collections.Generic;

namespace Ev3Dev.CSharp
{
	public class Motor : Device
	{
		public Motor( string address )
		{
			var success = Connect( new Dictionary<string, string[]>
			{
				{ AddressAttribute, new[] { address } }
			} );

			if ( !success )
			{ throw new ArgumentException( $"Motor at {address} is not found" ); }
		}

		public Motor( string address, string motorType )
		{
			var success = Connect( new Dictionary<string, string[]>
			{
				{ AddressAttribute, new[] { address } },
				{ DriverNameAttribute, new[] { motorType } }
			} );

			if ( !success )
			{ throw new ArgumentException( $"Motor {motorType} at {address} is not found" ); }
		}

		protected bool Connect( IDictionary<string, string[]> matchCriteria )
		{
			string path = $@"{SysRoot}/{TachoMotorClass}/";
			return Connect( path, MotorPattern, matchCriteria );
		}

		/// <summary>
		/// Returns the name of the port that this motor is connected to.
		/// </summary>
		public string Address => GetStringAttribute( AddressAttribute );

		/// <summary>
		/// Sends a command to the motor controller. See `commands` for a list of possible values.
		/// </summary>
		public string Command { set { SetStringAttribute( CommandAttribute, value ); } }

		/// <summary>
		/// Returns a list of commands that are supported by the motor controller. Possible values are 
		/// `run-forever`, `run-to-abs-pos`, `run-to-rel-pos`, `run-timed`, `run-direct`, `stop` and `reset`. 
		/// These commands are provided as string constants.
		/// Not all commands may be supported.
		/// </summary>
		public string[] Commands => GetStringArrayAttribute( CommandsAttribute );

		/// <summary>
		/// Returns the number of tacho counts in one rotation of the motor. 
		/// Tacho counts are used by the position and speed attributes, 
		/// so you can use this value to convert rotations or degrees to tacho counts. 
		/// In the case of linear actuators, the units here will be counts per centimeter.
		/// </summary>
		public int CountPerRot => GetIntAttribute( CountPerRotAttribute );

		/// <summary>
		/// Returns the name of the driver that provides this tacho motor device.
		/// </summary>
		public string DriverName => GetStringAttribute( DriverNameAttribute );

		/// <summary>
		/// Returns the current duty cycle of the motor. Units are percent. Values are -100 to 100.
		/// </summary>
		public int DutyCycle => GetIntAttribute( DutyCycleAttribute );

		/// <summary>
		/// Writing sets the duty cycle setpoint. Reading returns the current value. 
		/// Units are in percent. Valid values are -100 to 100. 
		/// A negative value causes the motor to rotate in reverse. 
		/// This value is only used when speed_regulation is off.
		/// </summary>
		public int DutyCycleSp
		{
			get { return GetIntAttribute( DutyCycleSpAttribute ); }
			set { SetIntAttribute( DutyCycleSpAttribute, value ); }
		}

		/// <summary>
		/// Sets the polarity of the rotary encoder. 
		/// This is an advanced feature to all use of motors that send inversed encoder signals to the EV3. 
		/// This should be set correctly by the driver of a device. 
		/// It You only need to change this value if you are using a unsupported device. 
		/// Valid values are normal and inversed.
		/// </summary>
		public string EncoderPolarity
		{
			get { return GetStringAttribute( EncoderPolarityAttribute ); }
			set { SetStringAttribute( EncoderPolarityAttribute, value ); }
		}

		/// <summary>
		/// Sets the polarity of the motor. 
		/// With normal polarity, a positive duty cycle will cause the motor to rotate clockwise. 
		/// With inversed polarity, a positive duty cycle will cause the motor to rotate counter-clockwise. 
		/// Valid values are normal and inversed.
		/// </summary>
		public string Polarity
		{
			get { return GetStringAttribute( PolarityAttribute ); }
			set { SetStringAttribute( PolarityAttribute, value ); }
		}

		/// <summary>
		/// Returns the current position of the motor in pulses of the rotary encoder. 
		/// When the motor rotates clockwise, the position will increase. 
		/// Likewise, rotating counter-clockwise causes the position to decrease. 
		/// Writing will set the position to that value.
		/// </summary>
		public int Position
		{
			get { return GetIntAttribute( PositionAttribute ); }
			set { SetIntAttribute( PositionAttribute, value ); }
		}

		/// <summary>
		/// The proportional constant for the position PID.
		/// </summary>
		public int PositionP
		{
			get { return GetIntAttribute( PositionPAttribute ); }
			set { SetIntAttribute( PositionPAttribute, value ); }
		}

		/// <summary>
		/// The integral constant for the position PID.
		/// </summary>
		public int PositionI
		{
			get { return GetIntAttribute( PositionIAttribute ); }
			set { SetIntAttribute( PositionIAttribute, value ); }
		}

		/// <summary>
		/// The derivative constant for the position PID.
		/// </summary>
		public int PositionD
		{
			get { return GetIntAttribute( PositionDAttribute ); }
			set { SetIntAttribute( PositionDAttribute, value ); }
		}

		/// <summary>
		/// Writing specifies the target position for the run-to-abs-pos and run-to-rel-pos commands. 
		/// Reading returns the current value. Units are in tacho counts. 
		/// You can use the value returned by counts_per_rot to convert tacho counts to/from rotations or degrees.
		/// </summary>
		public int PositionSp
		{
			get { return GetIntAttribute( PositionSpAttribute ); }
			set { SetIntAttribute( PositionSpAttribute, value ); }
		}

		/// <summary>
		/// Returns the current motor speed in tacho counts per second. 
		/// Note, this is not necessarily degrees (although it is for LEGO motors). 
		/// Use the count_per_rot attribute to convert this value to RPM or deg/sec.
		/// </summary>
		public int Speed => GetIntAttribute( SpeedAttribute );

		/// <summary>
		/// Writing sets the target speed in tacho counts per second used when speed_regulation is on. 
		/// Reading returns the current value. 
		/// Use the count_per_rot attribute to convert RPM or deg/sec to tacho counts per second.
		/// </summary>
		public int SpeedSp
		{
			get { return GetIntAttribute( SpeedSpAttribute ); }
			set { SetIntAttribute( SpeedSpAttribute, value ); }
		}

		/// <summary>
		/// Writing sets the ramp up setpoint. Reading returns the current value. 
		/// Units are in milliseconds. When set to a value > 0, 
		/// the motor will ramp the power sent to the motor from 0 to 100% duty cycle over the span 
		/// of this setpoint when starting the motor. 
		/// If the maximum duty cycle is limited by duty_cycle_sp or speed regulation, 
		/// the actual ramp time duration will be less than the setpoint.
		/// </summary>
		public int RampUpSp
		{
			get { return GetIntAttribute( RampUpSpAttribute ); }
			set { SetIntAttribute( RampUpSpAttribute, value ); }
		}

		/// <summary>
		/// Writing sets the ramp down setpoint. Reading returns the current value. 
		/// Units are in milliseconds. When set to a value > 0, 
		/// the motor will ramp the power sent to the motor from 100% duty cycle down to 0 over the span 
		/// of this setpoint when stopping the motor. If the starting duty cycle is less than 100%, 
		/// the ramp time duration will be less than the full span of the setpoint.
		/// </summary>
		public int RampDownSp
		{
			get { return GetIntAttribute( RampDownSpAttribute ); }
			set { SetIntAttribute( RampDownSpAttribute, value ); }
		}

		/// <summary>
		/// Turns speed regulation on or off. If speed regulation is on, 
		/// the motor controller will vary the power supplied to the motor to try to maintain the speed specified in speed_sp. 
		/// If speed regulation is off, the controller will use the power specified in duty_cycle_sp. Valid values are on and off.
		/// </summary>
		public string SpeedRegulationEnabled
		{
			get { return GetStringAttribute( SpeedRegulationAttribute ); }
			set { SetStringAttribute( SpeedRegulationAttribute, value ); }
		}

		/// <summary>
		/// The proportional constant for the speed regulation PID.
		/// </summary>
		public int SpeedRegulationP
		{
			get { return GetIntAttribute( SpeedRegulationPAttribute ); }
			set { SetIntAttribute( SpeedRegulationPAttribute, value ); }
		}

		/// <summary>
		/// The integral constant for the speed regulation PID.
		/// </summary>
		public int SpeedRegulationI
		{
			get { return GetIntAttribute( SpeedRegulationIAttribute ); }
			set { SetIntAttribute( SpeedRegulationIAttribute, value ); }
		}

		/// <summary>
		/// The derivative constant for the speed regulation PID.
		/// </summary>
		public int SpeedRegulationD
		{
			get { return GetIntAttribute( SpeedRegulationDAttribute ); }
			set { SetIntAttribute( SpeedRegulationDAttribute, value ); }
		}

		/// <summary>
		/// Reading returns a list of state flags. Possible flags are `running`, `ramping` `holding` and `stalled`.
		/// </summary>
		public string[] State => GetStringArrayAttribute( StateAttribute );

		/// <summary>
		/// Reading returns the current stop command. Writing sets the stop command. 
		/// The value determines the motors behavior when command is set to `stop`. 
		/// Also, it determines the motors behavior when a run command completes. 
		/// See `stop_commands` for a list of possible values.
		/// </summary>
		public string StopCommand
		{
			get { return GetStringAttribute( StopCommandAttribute ); }
			set { SetStringAttribute( StopCommandAttribute, value ); }
		}

		/// <summary>
		/// Returns a list of stop modes supported by the motor controller. Possible values are `coast`, `brake` and `hold`. 
		/// `coast` means that power will be removed from the motor and it will freely coast to a stop. 
		/// `brake` means that power will be removed from the motor and a passive electrical load will be placed on the motor. 
		/// This is usually done by shorting the motor terminals together. 
		/// This load will absorb the energy from the rotation of the motors and cause the motor to stop more quickly than coasting. 
		/// `hold` does not remove power from the motor. Instead it actively try to hold the motor at the current position. 
		/// If an external force tries to turn the motor, the motor will ‘push back’ to maintain its position.
		/// </summary>
		public string[] StopCommands => GetStringArrayAttribute( StopCommandsAttribute );

		/// <summary>
		/// Writing specifies the amount of time the motor will run when using the run-timed command. 
		/// Reading returns the current value. Units are in milliseconds.
		/// </summary>
		public int TimeSp
		{
			get { return GetIntAttribute( TimeSpAttribute ); }
			set { SetIntAttribute( TimeSpAttribute, value ); }
		}

		public const string LargeMotorDriver = "lego-ev3-l-motor";
		public const string MediumMotorDriver = "lego-ev3-m-motor";

		public const string CommandAttribute = "command";
		public const string CommandsAttribute = "commands";
		public const string StopCommandsAttribute = "stop_commands";
		public const string CountPerRotAttribute = "count_per_rot";
		public const string DriverNameAttribute = "driver_name";
		public const string DutyCycleAttribute = "duty_cycle";
		public const string DutyCycleSpAttribute = "duty_cycle_sp";
		public const string EncoderPolarityAttribute = "encoder_polarity";
		public const string PolarityAttribute = "polarity";
		public const string AddressAttribute = "address";
		public const string PositionAttribute = "position";
		public const string PositionPAttribute = "hold_pid/Kp";
		public const string PositionIAttribute = "hold_pid/Ki";
		public const string PositionDAttribute = "hold_pid/Kd";
		public const string PositionSpAttribute = "position_sp";
		public const string SpeedAttribute = "speed";
		public const string SpeedSpAttribute = "speed_sp";
		public const string RampUpSpAttribute = "ramp_up_sp";
		public const string RampDownSpAttribute = "ramp_down_sp";
		public const string SpeedRegulationAttribute = "speed_regulation";
		public const string SpeedRegulationPAttribute = "speed_pid/Kp";
		public const string SpeedRegulationIAttribute = "speed_pid/Ki";
		public const string SpeedRegulationDAttribute = "speed_pid/Kd";
		public const string StateAttribute = "state";
		public const string StopCommandAttribute = "stop_command";
		public const string TimeSpAttribute = "time_sp";
		
		private const string TachoMotorClass = @"tacho-motor";
		private const string MotorPattern = @"motor";
	}

	/// <summary>
	/// Provides string constants for motor commands.
	/// </summary>
	public static class MotorCommands
	{
		/// <summary>
		/// Run the motor until another command is sent.
		/// </summary>
		public const string CommandRunForever = "run-forever";

		/// <summary>
		/// Run to an absolute position specified by `position_sp` and then
		/// stop using the command specified in `stop_command`. 
		/// </summary>
		public const string CommandRunToAbsPos = "run-to-abs-pos";

		/// <summary>
		/// Run to a position relative to the current `position` value.
		/// The new position will be current `position` + `position_sp`.
		/// When the new position is reached, the motor will stop using
		/// the command specified by `stop_command`.
		/// </summary> 
		public const string CommandRunToRelPos = "run-to-rel-pos";

		/// <summary>
		/// Run the motor for the amount of time specified in `time_sp`
		/// and then stop the motor using the command specified by `stop_command`.
		/// </summary>
		public const string CommandRunTimed = "run-timed";

		/// <summary>
		/// Run the motor at the duty cycle specified by `duty_cycle_sp`.
		/// Unlike other run commands, changing `duty_cycle_sp` while running *will*
		/// take effect immediately.
		/// </summary>
		public const string CommandRunDirect = "run-direct";

		/// <summary>
		/// Stop any of the run commands before they are complete using the
		/// command specified by `stop_command`.
		/// </summary>
		public const string CommandStop = "stop";

		/// <summary>
		/// Reset all of the motor parameter attributes to their default value.
		/// This will also have the effect of stopping the motor.
		/// </summary>
		public const string CommandReset = "reset";

		/// <summary>
		/// Power will be removed from the motor and it will freely coast to a stop.
		/// </summary>
		public const string StopCommandCoast = "coast";

		/// <summary>
		/// Power will be removed from the motor and a passive electrical load will
		/// be placed on the motor. This is usually done by shorting the motor terminals
		/// together. This load will absorb the energy from the rotation of the motors and
		/// cause the motor to stop more quickly than coasting.
		/// </summary>
		public const string StopCommandBrake = "brake";

		/// <summary>
		/// Does not remove power from the motor. Instead it actively try to hold the motor
		/// at the current position. If an external force tries to turn the motor, the motor
		/// will ``push back`` to maintain its position.
		/// </summary>
		public const string StopCommandHold = "hold";

		/// <summary>
		/// Sets the normal polarity of the rotary encoder.
		/// </summary>
		public const string EncoderPolarityNormal = "normal";

		/// <summary>
		/// Sets the inversed polarity of the rotary encoder.
		/// </summary>
		public const string EncoderPolarityInversed = "inversed";

		/// <summary>
		/// With `normal` polarity, a positive duty cycle will
		/// cause the motor to rotate clockwise.
		/// </summary>
		public const string PolarityNormal = "normal";

		/// <summary>
		/// With `inversed` polarity, a positive duty cycle will
		/// cause the motor to rotate counter-clockwise.
		/// </summary>
		public const string PolarityInversed = "inversed";

		/// <summary>
		/// The motor controller will vary the power supplied to the motor
		/// to try to maintain the speed specified in `speed_sp`.
		/// </summary>
		public const string SpeedRegulationOn = "on";

		/// <summary>
		/// The motor controller will use the power specified in `duty_cycle_sp`.
		/// </summary>
		public const string SpeedRegulationOff = "off";
	}
}
