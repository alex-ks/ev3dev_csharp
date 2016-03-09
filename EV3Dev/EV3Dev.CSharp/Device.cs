using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ev3Dev.CSharp
{
	public abstract class Device : IDisposable
    {
	    private string _path;
	    private int _deviceIndex = -1;
	    private readonly IDictionary<string, StreamWriter> _writableAttributes;

	    protected Device( )
	    {
		    _writableAttributes = new Dictionary<string, StreamWriter>( );
	    }

	    public bool Connected => !string.IsNullOrEmpty( _path );

	    public int DeviceIndex
	    {
		    get
		    {
			    if ( _deviceIndex < 0 )
			    {
				    int rank = 1;
				    _deviceIndex = 0;
				    foreach ( var c in _path.Where( char.IsDigit ) )
				    {
					    _deviceIndex += ( int )char.GetNumericValue( c ) * rank;
					    rank *= 10;
				    }
			    }

			    return _deviceIndex;
		    }
	    }

	    protected string GetStringAttribute( string attributeName )
	    {
		    if ( !Connected )
		    { throw new InvalidOperationException( "Device is not connected" ); }

		    using ( var stream = new FileStream( Path.Combine( _path, attributeName ), FileMode.Open, FileAccess.Read ) )
		    {
			    using ( var reader = new StreamReader( stream ) )
			    {
				    return reader.ReadToEnd( );
			    }
		    }
	    }

		protected string[] GetStringArrayAttribute( string attributeName )
		{
			return GetStringAttribute( attributeName ).Split( );
		}

		protected string[] GetStringSelectorAttribute( string attributeName, out string selected )
		{
			var variants = GetStringArrayAttribute( attributeName );
			selected = null;

			for ( int i = 0; i < variants.Length; ++i )
			{
				if ( variants[i].StartsWith( "[" ) && variants[i].EndsWith( "]" ) )
				{
					selected = variants[i].Substring( 1, variants[i].Length - 2 );
					variants[i] = selected;
					break;
				}
			}
			
			return variants;
		}

	    protected void SetStringAttribute( string attributeName, string value )
	    {
		    StreamWriter writer;
		    if ( !_writableAttributes.ContainsKey( attributeName ) )
		    {
				writer = new StreamWriter( Path.Combine( _path, attributeName ) );
				_writableAttributes.Add( attributeName, writer );
		    }
		    else
		    {
			    writer = _writableAttributes[attributeName];
		    }
			writer.Write( value );
		    writer.Flush( );
	    }

	    protected void SetIntAttribute( string attributeName, int value )
	    {
		    SetStringAttribute( attributeName, value.ToString( ) );
	    }

	    protected int GetIntAttribute( string attributeName )
	    {
		    return int.Parse( GetStringAttribute( attributeName ) );
	    }

	    protected bool Connect( string classDirectory, string pattern, IDictionary<string, string[]> matchCriteria )
	    {
			if ( !Directory.Exists( classDirectory ) )
			{ return false; }

		    var directories = Directory.EnumerateDirectories( classDirectory );

		    foreach ( var directory in directories )
		    {
			    var directoryName = Path.GetDirectoryName( directory );
			    if ( directoryName != null && directoryName.StartsWith( pattern ) )
			    {
				    bool match = true;

				    foreach ( var matchCriterion in matchCriteria )
				    {
					    using ( var attributeStream = new FileStream( $@"{directory}/{matchCriterion.Key}", FileMode.Open, FileAccess.Read ) )
					    {
						    using ( var reader = new StreamReader( attributeStream ) )
						    {
							    var value = reader.ReadLine( );
							    if ( !matchCriterion.Value.Any( x => value.Equals( x ) ) )
							    {
								    match = false;
								    break;
							    }
						    }
					    }
				    }

				    if ( match )
				    {
					    _path = directory;
					    return true;
				    }
			    }
		    }

		    return false;
	    }

	    public virtual void Dispose( )
	    {
		    foreach ( var stream in _writableAttributes )
		    {
			    stream.Value.Dispose( );
		    }
	    }

		public const string SysRoot = @"/sys/class";
    }
}
