using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.BasicDevices
{
	public class MediumMotor : LegoMotor
	{
		public MediumMotor( OutputPort port ) : base( port, MediumMotorDriver )
		{
			
		}
	}
}
