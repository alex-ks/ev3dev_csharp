using System;
using System.Collections.Generic;

namespace Ev3Dev.CSharp.Demos
{
	public class SoundDemo
	{
		static IEnumerable<BeepDesc> ImperialAnthem( )
		{
			yield return new BeepDesc( Tones.F, 300, 200 );
			yield return new BeepDesc( Tones.F, 300, 200 );
			yield return new BeepDesc( Tones.F, 300, 200 );
			yield return new BeepDesc( Tones.C, 200, 167 );
			yield return new BeepDesc( Tones.Gis, 100, 67 );
			yield return new BeepDesc( Tones.F, 300, 200 );
			yield return new BeepDesc( Tones.C, 200, 167 );
			yield return new BeepDesc( Tones.Gis, 100, 67 );
			yield return new BeepDesc( Tones.F, 300, 200 );
		}

		public static void Main( string[] args )
		{
			Console.Out.WriteLine( "Beeping C#" );
			Sound.Tone( Tones.Cis, 50, true );
			Console.Out.WriteLine( "Playing sequence" );
			Sound.Tone( ImperialAnthem( ), true );
			Console.Out.WriteLine( "Speaking" );
			Sound.Speak( "Good bye", 120, 200, true );
		}
	}
}
