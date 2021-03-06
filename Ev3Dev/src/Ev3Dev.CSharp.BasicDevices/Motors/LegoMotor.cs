﻿using System;
using System.Threading;

namespace Ev3Dev.CSharp.BasicDevices.Motors
{
    public enum Polarity
    {
        Normal, Inversed
    }

    public enum StopAction
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

    /// <summary>
    /// Determines how motor will detect Run* task execution finish.
    /// </summary>
    public enum SynchronizationMode
    {
        /// <summary>
        /// Command completion waiting will be finished when the motor speed is equal to zero.
        /// </summary>
        WaitForStop,
        /// <summary>
        /// Command completion waiting will be finished when the command conditions are met.
        /// </summary>
        WaitForCompletion
    }

    public class LegoMotor : Motor
    {
        protected LegoMotor(OutputPort port, string motorType) : base(port.ToStringName(), motorType)
        {

        }

        /// <summary>
        /// Determines command completion waiting behaviour
        /// (see <see cref="Motors.SynchronizationMode"/> for details).
        /// Default mode is <see cref="Motors.SynchronizationMode.WaitForCompletion"/>.
        /// </summary>
        public SynchronizationMode SynchronizationMode { get; set; } = SynchronizationMode.WaitForCompletion;

        /// <summary>
        /// Period of motor polling when waiting for motor stop in milliseconds.
        /// Matters only if <see cref="SynchronizationMode"/> is equal to
        /// <see cref="Motors.SynchronizationMode.WaitForStop"/>.
        /// Default period is 50 ms.
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
        /// The value determines the motors behavior when command <see cref="Stop()"/> is called.
        /// Also, it determines the motors behavior when a run command completes.
        /// </summary>
        public new StopAction StopAction
        {
            get { return StringToStopAction(base.StopAction); }
            set { base.StopAction = StopActionToString(value); }
        }

        /// <summary>
        /// Run the motor until another command is called.
        /// </summary>
        public void RunForever()
        {
            Command = MotorCommands.CommandRunForever;
        }

        /// <summary>
        /// Run the motor with specified speed until another command is called.
        /// </summary>
        /// <param name="speed">Motor speed in degrees per second.</param>
        public void RunForever(int speed)
        {
            var old = SpeedSp;
            var tachoSpeed = (int)(speed * (CountPerRot / 360.0));

            try
            {
                if (old != tachoSpeed)
                    SpeedSp = tachoSpeed;
                Command = MotorCommands.CommandRunForever;
            }
            finally
            {
                // to avoid double throw
                try
                {
                    if (tachoSpeed != old)
                        SpeedSp = old;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Run the motor for the specified amount of time
        /// and then stop the motor using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="ms">Time in milleseconds.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask RunTimed(int ms)
        {
            TimeSp = ms;
            var start = DateTime.Now;
            Command = MotorCommands.CommandRunTimed;

            Action waiter;
            if (SynchronizationMode == SynchronizationMode.WaitForCompletion)
            { waiter = () => WaitForPeriod(start, ms); }
            else
            { waiter = WaitForStop; }

            return new LazyTask(waiter);
        }

        /// <summary>
        /// Run the motor for the specified amount of time with the specified speed
        /// and then stop the motor using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="ms">Time in milleseconds.</param>
        /// <param name="speed">Motor speed in degrees per second.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask RunTimed(int ms, int speed)
        {
            var old = SpeedSp;
            var tachoSpeed = (int)(speed * (CountPerRot / 360.0));

            try
            {
                if (old != tachoSpeed)
                    SpeedSp = tachoSpeed;

                var start = DateTime.Now;
                RunTimed(ms);

                return new LazyTask(() => WaitForPeriod(start, ms));
            }
            finally
            {
                // to avoid double throw
                try
                {
                    if (old != tachoSpeed)
                        SpeedSp = old;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Stop any of the run commands before they are complete using the
        /// action specified by <see cref="StopAction"/> property.
        /// </summary>
        public void Stop()
        {
            Command = MotorCommands.CommandStop;
        }

        /// <summary>
        /// Stop any of the run commands before they are complete using the specified stop action.
        /// </summary>
        /// <param name="action">Describes how to stop the motor.</param>
        public void Stop(StopAction action)
        {
            var old = StopAction;
            if (action != old)
            { StopAction = action; }
            Stop();
            if (action != old)
            { StopAction = old; }
        }

        /// <summary>
        /// Reset all of the motor parameter attributes to their default value.
        /// This will also have the effect of stopping the motor.
        /// </summary>
        public void Reset()
        {
            Command = MotorCommands.CommandReset;
        }

        /// <summary>
        /// Run the motor until the <see cref="Motor.Position"/> reaches specified value
        /// using the action specified by <see cref="StopAction"/> property.
        /// Note: motor position becomes equal to zero when <see cref="Reset"/> is called.
        /// </summary>
        /// <param name="position">Motor position in tacho counts.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask RunToPosition(int position)
        {
            PositionSp = position;
            Command = MotorCommands.CommandRunToAbsPos;
            var currentPosition = Position;

            Action waiter;
            if (SynchronizationMode == SynchronizationMode.WaitForCompletion)
            { waiter = () => WaitForPosition(position, position - currentPosition > 0); }
            else
            { waiter = WaitForStop; }

            return new LazyTask(waiter);
        }

        /// <summary>
        /// Run the motor until the <see cref="Motor.Position"/> reaches specified value
        /// with the specified speed and then stop the motor
        /// using the action specified by <see cref="StopAction"/> property.
        /// Note: motor position becomes equal to zero when <see cref="Reset"/> is called.
        /// </summary>
        /// <param name="position">Motor position in tacho counts.</param>
        /// <param name="speed">Motor speed in degrees per second.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask RunToPosition(int position, int speed)
        {
            return BackupSpeedAndRun(speed, () => RunToPosition(position));
        }

        /// <summary>
        /// Turn the motor on specified amount of degrees and then stop the motor
        /// using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="degrees">Amount of degrees to turn the motor.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask Run(int degrees)
        {
            var setPoint = (int)(degrees * (CountPerRot / 360.0));
            PositionSp = setPoint;
            var finish = Position + setPoint;
            Command = MotorCommands.CommandRunToRelPos;

            Action waiter;
            if (SynchronizationMode == SynchronizationMode.WaitForCompletion)
            { waiter = () => WaitForPosition(finish, degrees > 0); }
            else
            { waiter = WaitForStop; }

            return new LazyTask(waiter);
        }

        /// <summary>
        /// Turn the motor on specified amount of degrees  with the specified speed and then stop the motor
        /// using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="degrees">Amount of degrees to turn the motor.</param>
        /// <param name="speed">Motor speed in degrees per second.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask Run(int degrees, int speed)
        {
            return BackupSpeedAndRun(speed, () => Run(degrees));
        }

        /// <summary>
        /// Turn the motor on specified amount of rotations and then stop the motor
        /// using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="rotations">Amount of rotations to turn the motor.</param>
        /// <returns>A <see cref="LazyTask"/> to wait the execution competion.</returns>
        public LazyTask Run(float rotations)
        {
            var setPoint = (int)(rotations * CountPerRot);
            PositionSp = setPoint;
            var finish = Position + setPoint;
            Command = MotorCommands.CommandRunToRelPos;

            Action waiter;
            if (SynchronizationMode == SynchronizationMode.WaitForCompletion)
            { waiter = () => WaitForPosition(finish, rotations > 0); }
            else
            { waiter = WaitForStop; }

            return new LazyTask(waiter);
        }

        /// <summary>
        /// Turn the motor on specified amount of rotations with the specified speed
        /// and then stop the motor using the action specified by <see cref="StopAction"/> property.
        /// </summary>
        /// <param name="rotations">Rotations count</param>
        /// <param name="speed">Motor speed in degrees per second.</param>
        /// <returns></returns>
        public LazyTask Run(float rotations, int speed)
        {
            return BackupSpeedAndRun(speed, () => Run(rotations));
        }

        /// <summary>
        /// Run the motor at the duty cycle specified by <see cref="Motor.DutyCycleSp"/>.
        /// Unlike other run commands, changing <see cref="Motor.DutyCycleSp"/> while running *will*
        /// take effect immediately.
        /// </summary>
        public void RunDirect()
        {
            Command = MotorCommands.CommandRunDirect;
        }

        /// <summary>
        /// Run the motor with specified power until another command is called.
        /// </summary>
        /// <param name="power">Motor power in percents (from -100 to 100).</param>
        /// <remarks>
        /// This method reads old value of <see cref="Motor.DutyCycleSp"/> and if it differs from the arguments,
        /// sets <see cref="Motor.DutyCycleSp"/> to power and then restores the old value in case of an error.
        /// </remarks>
        public void RunDirect(sbyte power)
        {
            var old = DutyCycleSp;

            try
            {
                if (old != power)
                    DutyCycleSp = power;
                Command = MotorCommands.CommandRunDirect;
            }
            finally
            {
                // to avoid double throw
                try
                {
                    if (old != power)
                        DutyCycleSp = old;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        //todo: check DutySycleSp while "hold" stop action
        /// <summary>
        /// Wait until the motor speed is equal to 0.
        /// </summary>
        public void WaitForStop()
        {
            while (Speed != 0)
            {
                Thread.Sleep(PollPeriod);
            }
        }

        public void WaitForPeriod(DateTime start, int ms)
        {
            var remain = ms - (int)(DateTime.Now - start).TotalMilliseconds;
            if (remain > 0)
            { Thread.Sleep(remain); }
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
        public void WaitForPosition(int position, bool clockwise)
        {
            Func<int, bool> stopCriterion;
            if (clockwise)
            { stopCriterion = curr => curr >= position; }
            else
            { stopCriterion = curr => curr <= position; }

            while (stopCriterion(Position))
            {
                Thread.Sleep(PollPeriod);
            }
        }

        private LazyTask BackupSpeedAndRun(int speed, Func<LazyTask> action)
        {
            var old = SpeedSp;
            var tachoSpeed = (int)(speed * (CountPerRot / 360.0));

            try
            {
                if (old != tachoSpeed)
                    SpeedSp = tachoSpeed;
                return action();
            }
            finally
            {
                // to avoid double throw
                try
                {
                    if (old != tachoSpeed)
                        SpeedSp = old;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private static StopAction StringToStopAction(string action)
        {
            switch (action)
            {
                case MotorCommands.StopActionCoast:
                    return StopAction.Coast;
                case MotorCommands.StopActionBrake:
                    return StopAction.Brake;
                case MotorCommands.StopActionHold:
                    return StopAction.Hold;
                default:
                    return StopAction.Coast;
            }
        }

        private static string StopActionToString(StopAction action)
        {
            switch (action)
            {
                case StopAction.Coast:
                    return MotorCommands.StopActionCoast;
                case StopAction.Brake:
                    return MotorCommands.StopActionBrake;
                case StopAction.Hold:
                    return MotorCommands.StopActionHold;
                default:
                    return MotorCommands.StopActionCoast;
            }
        }
    }
}
