using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.BasicDevices
{
	public class LargeMotor : LegoMotor
	{
		public LargeMotor( OutputPort port ) : base( port, LargeMotorDriver )
		{
			Reset( );
		}
	}
}
