namespace Ev3Dev.CSharp.BasicDevices.Motors
{
	public class LargeMotor : LegoMotor
	{
		public LargeMotor( OutputPort port ) : base( port, LargeMotorDriver )
		{
			Reset( );
		}
	}
}
