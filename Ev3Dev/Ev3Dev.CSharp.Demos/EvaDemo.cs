using Ev3Dev.CSharp.BasicDevices;
using Ev3Dev.CSharp.BasicDevices.Motors;
using Ev3Dev.CSharp.BasicDevices.Sensors;
using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.Demos
{
    public class DiscoveryCar : IDisposable
    {
        private LargeMotor _leftMotor, _rightMotor;
        private MediumMotor _steeringMotor;

        private ColorSensor _colorSensor;
        private TouchSensor _touchSensor;
        private InfraredSensor _infraredSensor;
        
        public bool IsDark => _colorSensor.LightIntensity < 10;

        [ShutdownEvent]
        public bool Touched => _touchSensor.State == TouchSensorState.Pressed;

        // There is conversion from bool to int in order to avoid bool switch ambiguity.
        // Switch is used here to activate handler only twice: when button is pressed and
        // when button is released.
        // Otherwise, handler would be activated on each loop iteration while button is pressed.
        [Switch]
        public int ForwardRequired => _infraredSensor.RedDownPressed( ) ? 1 : 0;
        [Switch]
        public int BackwardRequired => _infraredSensor.BlueDownPressed( ) ? 1 : 0;
        [Switch]
        public int LeftTurnRequired => _infraredSensor.RedUpPressed( ) ? 1 : 0;
        [Switch]
        public int RightTurnRequired => _infraredSensor.BlueUpPressed( ) ? 1 : 0;

        [EventHandler( nameof( ForwardRequired ) )]
        public void DriveForward( [FromSource( nameof( ForwardRequired ) )] int forwardRequired,
                                  [FromSource( nameof( BackwardRequired ) )] int backwardRequired )
        {
            if ( backwardRequired == 1 )
                return;
            if ( forwardRequired == 1 )
                Move( power: 75 );
            else
                Stop( );
        }

        [EventHandler( nameof( BackwardRequired ) )]
        public void DriveBackward( [FromSource( nameof( ForwardRequired ) )] int forwardRequired,
                                   [FromSource( nameof( BackwardRequired ) )] int backwardRequired )
        {
            if ( forwardRequired == 1 )
                return;
            if ( backwardRequired == 1 )
                Move( power: -75 );
            else
                Stop( );
        }

        [EventHandler( nameof( LeftTurnRequired ) )]
        public void TurnLeft( [FromSource( nameof( LeftTurnRequired ) )] int leftRequired,
                              [FromSource( nameof( RightTurnRequired ) )] int rightRequired )
        {
            if ( rightRequired == 1 )
                return;
            if ( leftRequired == 1 )
                _steeringMotor.RunForever( power: -30 );
            else
                _steeringMotor.RunToPosition( position: 0, power: 30 );
        }

        [EventHandler( nameof( LeftTurnRequired ) )]
        public void TurnRight( [FromSource( nameof( LeftTurnRequired ) )] int leftRequired,
                               [FromSource( nameof( RightTurnRequired ) )] int rightRequired )
        {
            if ( leftRequired == 1 )
                return;
            if ( rightRequired == 1 )
                _steeringMotor.RunForever( power: 30 );
            else
                _steeringMotor.RunToPosition( position: 0, power: -30 );
        }

        [NonCritical]
        [NonReenterable]
        [EventHandler( nameof( IsDark ) )]
        public async Task FearDark( )
        {
            await Sound.Speak( "It's dark, I'm scared", wordsPerMinute: 130, amplitude: 300 );
        }

        private void Move( sbyte power )
        {
            _leftMotor.RunForever( power );
            _rightMotor.RunForever( power );
        }

        private void Stop( )
        {
            _leftMotor.Stop( );
            _rightMotor.Stop( );
        }

        public DiscoveryCar( )
        {
            _leftMotor = new LargeMotor( OutputPort.OutD )
            {
                StopCommand = StopCommand.Brake
            };
            _rightMotor = new LargeMotor( OutputPort.OutA )
            {
                StopCommand = StopCommand.Brake
            };
            _steeringMotor = new MediumMotor( OutputPort.OutB )
            {
                StopCommand = StopCommand.Hold
            };

            _colorSensor = new ColorSensor( InputPort.In4 )
            {
                Mode = ColorSensorMode.AmbientLight
            };
            _touchSensor = new TouchSensor( InputPort.In1 );
            _infraredSensor = new InfraredSensor( InputPort.In3 )
            {
                Mode = InfraredSensorMode.IrRemoteControlAlternative
            };

            // steering motor calibration

            _steeringMotor.RunTimed( ms: 500, power: 50 );
            var right = _steeringMotor.Position;
            _steeringMotor.RunTimed( ms: 500, power: -50 );
            var left = _steeringMotor.Position;
            _steeringMotor.RunToPosition( position: ( right - left ) / 2, power: 50 );
            _steeringMotor.Position = 0;
        }

        public void Dispose( )
        {
            using ( _leftMotor )
            using ( _rightMotor )
            using ( _steeringMotor )
            using ( _colorSensor )
            using ( _touchSensor )
            using ( _infraredSensor )
            {
                _leftMotor.Reset( );
                _rightMotor.Reset( );
                _steeringMotor.Reset( );
            }
        }
    }

    public class EvaDemo
    {
        public static void Main( )
        {
            var loop = new EventLoop( );
            using ( var car = new DiscoveryCar( ) )
            {
                loop.RegisterModel( car, treatMethodsAsCritical: true );
                loop.Start( millisecondsCooldown: 20 );
                Thread.Sleep( 2000 );
            }
        }
    }
}
