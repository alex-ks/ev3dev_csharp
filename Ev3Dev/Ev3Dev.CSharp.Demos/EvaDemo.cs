using Ev3Dev.CSharp.BasicDevices;
using Ev3Dev.CSharp.BasicDevices.Motors;
using Ev3Dev.CSharp.BasicDevices.Sensors;
using Ev3Dev.CSharp.EvA;
using System;
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
        private int _consoleLeft;
        private int _consoleTop;

        public bool IsDark => _colorSensor.LightIntensity < 2;

        [ShutdownEvent]
        public bool Touched => _touchSensor.State == TouchSensorState.Pressed;

        /* There is conversion from bool to int in order to avoid bool switch ambiguity.
         * Switch is used here to activate handler only twice: when button is pressed and
         * when button is released.
         * Otherwise, handler would be activated on each loop iteration while button is pressed.
         */
        [Switch]
        public int ForwardRequired => _infraredSensor.RedDownPressed( ) ? 1 : 0;
        public bool NoBackward => _leftMotor.Speed <= 0;

        [Switch]
        public int BackwardRequired => _infraredSensor.BlueDownPressed( ) ? 1 : 0;
        public bool NoForward => _leftMotor.Speed >= 0;

        [Switch]
        public int LeftTurnRequired => _infraredSensor.RedUpPressed( ) ? 1 : 0;
        public bool NoRightTurn => _steeringMotor.Speed <= 0;

        [Switch]
        public int RightTurnRequired => _infraredSensor.BlueUpPressed( ) ? 1 : 0;
        public bool NoLeftTurn => _steeringMotor.Speed >= 0;

        [EventHandler( nameof( ForwardRequired ), nameof( NoBackward ) )]
        public void DriveForward( [FromSource( nameof( ForwardRequired ) )] int forwardRequired )
        {
            if ( forwardRequired == 1 )
                Move( power: 75 );
            else
                Stop( );
        }

        [EventHandler( nameof( BackwardRequired ), nameof( NoForward ) )]
        public void DriveBackward( [FromSource( nameof( BackwardRequired ) )] int backwardRequired )
        {
            if ( backwardRequired == 1 )
                Move( power: -75 );
            else
                Stop( );
        }

        /*
         * If we release the button before the turning is completed,
         * two things will happen:
         * 1. The wheels won't come back to their position correctly, because new command will interrupt old one;
         * 2. First command waiting logic will be broken, because the wheels will never reach expected position,
         * so the working thread will poll the motor forever.
         * Because of this we make turning logic non-reenterable.
         * Also, we should't discard repeated calls, because in this case the wheels won't
         * come back to their original position. 
         */
        [NonReenterable( DiscardRepeated = false )]
        [EventHandler( nameof( LeftTurnRequired ), nameof( NoRightTurn ) )]
        public async Task TurnLeft( [FromSource( nameof( LeftTurnRequired ) )] int leftRequired )
        {
            if ( leftRequired == 1 )
                await _steeringMotor.Run( degrees: -45, power: 100 );
            else
                await _steeringMotor.Run( degrees: 45, power: 100 );
        }

        [NonReenterable( DiscardRepeated = false )]
        [EventHandler( nameof( RightTurnRequired ), nameof( NoLeftTurn ) )]
        public async Task TurnRight( [FromSource( nameof( RightTurnRequired ) )] int rightRequired )
        {
            if ( rightRequired == 1 )
                await _steeringMotor.Run( degrees: 45, power: 100 );
            else
                await _steeringMotor.Run( degrees: -45, power: 100 );
        }

        [NonCritical]
        [NonReenterable]
        [EventHandler( nameof( IsDark ) )]
        public async Task FearDark( )
        {
            await Sound.Speak( "It's dark, I'm scared", wordsPerMinute: 130, amplitude: 300 );
            await Task.Delay( 1000 );
        }

        // Uncomment this if you want to see values of properties
        //[Action]
        public void DebugOutput( )
        {
            Console.SetCursorPosition( _consoleLeft, _consoleTop );
            Console.WriteLine( $"{nameof( ForwardRequired )}: {ForwardRequired}" );
            Console.WriteLine( $"{nameof( BackwardRequired )}: {BackwardRequired}" );
            Console.WriteLine( $"{nameof( LeftTurnRequired )}: {LeftTurnRequired}" );
            Console.WriteLine( $"{nameof( RightTurnRequired )}: {RightTurnRequired}" );
            Console.WriteLine( $"{nameof( NoLeftTurn )}: {NoLeftTurn}" );
            Console.WriteLine( $"{nameof( NoRightTurn )}: {NoRightTurn}" );
            Console.WriteLine( $"Light: {_colorSensor.LightIntensity}" );
            Console.WriteLine( $"Steering: {_steeringMotor.Speed}" );
        }

        private void Move( sbyte power )
        {
            /* Interesting detail: sbyte uses int version of operator -, so -power has 
             * int type, not sbyte. Because of this, RunForever( -power ) calls version for speed (int), 
             * not for power (sbyte).
             */
            power *= -1;
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
            _leftMotor = new LargeMotor( OutputPort.OutD ) { StopCommand = StopCommand.Brake };
            _rightMotor = new LargeMotor( OutputPort.OutA ) { StopCommand = StopCommand.Brake };
            _steeringMotor = new MediumMotor( OutputPort.OutB ) { StopCommand = StopCommand.Hold };

            _colorSensor = new ColorSensor( InputPort.In4 ) { Mode = ColorSensorMode.AmbientLight };
            _touchSensor = new TouchSensor( InputPort.In1 );
            _infraredSensor = new InfraredSensor( InputPort.In3 )
            {
                Mode = InfraredSensorMode.IrRemoteControlAlternative
            };

            Console.WriteLine( "All devices connected" );
            _consoleLeft = Console.CursorLeft;
            _consoleTop = Console.CursorTop;
        }

        public void Dispose( )
        {
            /* There were used "using" constructions instead of direct Dispose( ) calls to ensure
             * that all objects will be disposed even if some method throws an exception.
             */
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
            Console.WriteLine( "Car turned off" );
        }
    }

    public class EvaDemo
    {
        public static void Main( )
        {
            using ( var car = new DiscoveryCar( ) )
            {
                var loop = car.BuildLoop( );
                Console.WriteLine( "Car components registered" );
                loop.Start( millisecondsCooldown: 5 );
                Thread.Sleep( 500 );
            }
        }
    }
}
