using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ev3Dev.CSharp
{
	/// <summary>
	/// Desribes tone by frequency, duration in milliseconds and delay before the next tone in milliseconds.
	/// </summary>
	public struct BeepDesc
	{
		public readonly float Frequency;
		public readonly float Ms;
		public readonly float Delay;

		public BeepDesc( float frequency, float ms, float delay )
		{
			Delay = delay;
			Ms = ms;
			Frequency = frequency;
		}

		public override string ToString( )
		{
			return $"-f {Frequency} -l {Ms} -D {Delay}";
		}
	}

	/// <summary>
	/// Provides static methods for simple sound operations.
	/// </summary>
	public static class Sound
	{
		/// <summary>
		/// Play single tone. Frequences of basic tones are defined as constants in <see cref="Tones"/> class.
		/// </summary>
		/// <param name="frequency">Frequency of tone in Hz. Default EV3 value is 440Hz.</param>
		/// <param name="ms">Duration of tone in milliseconds.</param>
		/// <param name="synchronously">Indicates whether process should wait for playback end.</param>
		public static void Tone( float frequency, float ms, bool synchronously )
		{
			string command = $"-f {frequency} -l {ms}";
			var proc = Process.Start( BeepPath, command );

			if ( synchronously )
			{ proc?.WaitForExit( ); }
		}

		/// <summary>
		/// Play sequence of tones.
		/// Frequences of basic tones are defined as constants in <see cref="Tones"/> class.
		/// </summary>
		/// <param name="sequence">Collection of tone descriptions. Tones will be played in the enumeration order.</param>
		/// <param name="synchronously">Indicates whether process should wait for playback end.</param>
		public static void Tone( IEnumerable<BeepDesc> sequence, bool synchronously )
		{
			StringBuilder builder = new StringBuilder( );
			bool first = true;

			foreach ( var beep in sequence )
			{
				if ( first )
				{ first = false; }
				else
				{ builder.Append( " -n " ); }

				builder.Append( beep );
			}
			
			var proc = Process.Start( BeepPath, builder.ToString( ) );

			if ( synchronously )
			{ proc?.WaitForExit( ); }
		}

		/// <summary>
		/// Play sound from file.
		/// </summary>
		/// <param name="soundFile">Name of file to play.</param>
		/// <param name="synchronously">Indicates whether process should wait for playback end.</param>
		public static void Play( string soundFile, bool synchronously )
		{
			var proc = Process.Start( APlayPath, $"-q {soundFile}" );

			if ( synchronously )
			{ proc?.WaitForExit( ); }
		}

		/// <summary>
		/// Speak text in English.
		/// </summary>
		/// <param name="text">Text in English to speak.</param>
		/// <param name="wordsPerMinute">Speech speed. Normal speed is 120-160 words per minute.</param>
		/// <param name="amplitude">Affects speech volume. For default EV3 speakers values greater than 1500 can cause distortion.</param>
		/// <param name="synchronously">Indicates whether process should wait for playback end.</param>
		public static void Speak( string text, int wordsPerMinute, int amplitude, bool synchronously )
		{
			string command = $"{ESpeakPath} -a {amplitude} -s {wordsPerMinute} --stdout \"{text}\" | {APlayPath} -q";

			var proc = Process.Start( BashPath, $"-c '{command}'" );

			if ( synchronously )
			{ proc?.WaitForExit( ); }
		}

		private const string BeepPath = "/usr/bin/beep";
		private const string APlayPath = "/usr/bin/aplay";
		private const string ESpeakPath = "/usr/bin/espeak";
		private const string BashPath = "/bin/bash";
	}

	/// <summary>
	/// Provides constants for basic tones frequences.
	/// </summary>
	public static class Tones
	{
		public const float C = 261.6f;
		public const float Cis = 277.2f;
		public const float D = 293.7f;
		public const float Dis = 311.1f;
		public const float E = 329.6f;
		public const float F = 349.2f;
		public const float Fis = 370.0f;
		public const float G = 392.0f;
		public const float Gis = 415.3f;
		public const float A = 440.0f;
		public const float Ais = 466.2f;
		public const float H = 493.9f;
		public const float CUp = 523.2f;
	}
}
