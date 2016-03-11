using System.Collections.Generic;

namespace Ev3Dev.CSharp.BasicDevices
{
	public enum InputPort
	{
		In1, In2, In3, In4
	}

	public enum OutputPort
	{
		OutA, OutB, OutC, OutD
	}

	public static class PortsHelper
	{
		private static readonly IDictionary<OutputPort, string> OutputPortNames;
		private static readonly IDictionary<InputPort, string> InputPortNames;

		static PortsHelper( )
		{
			OutputPortNames = new Dictionary<OutputPort, string>
			{
				{ OutputPort.OutA, "outA" },
				{ OutputPort.OutB, "outB" },
				{ OutputPort.OutC, "outC" },
				{ OutputPort.OutD, "outD" }
			};

			InputPortNames = new Dictionary<InputPort, string>
			{
				{ InputPort.In1, "in1" },
				{ InputPort.In2, "in2" },
				{ InputPort.In3, "in3" },
				{ InputPort.In4, "in4" }
			};
		}

		public static string ToStringName( this OutputPort port )
		{
			return OutputPortNames[port];
		}

		public static string ToStringName( this InputPort port )
		{
			return InputPortNames[port];
		}
	}
}
