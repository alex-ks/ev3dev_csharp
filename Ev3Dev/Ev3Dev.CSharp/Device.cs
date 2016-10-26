using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Ev3Dev.CSharp.Accessors;

namespace Ev3Dev.CSharp
{
	public abstract class Device : IDisposable
    {
        public static Func<IAttributeAccessor> PropertyAccessorProvider { get; set; } =
            ( ) => new AttributeAccessor( );

        private string _path;
	    private int _deviceIndex = -1;
        private readonly IAttributeAccessor _accessor;

	    protected Device( )
	    {
            _accessor = PropertyAccessorProvider( );
	    }

	    public bool Connected => !string.IsNullOrEmpty( _path );

	    public int DeviceIndex
	    {
		    get
		    {
                if ( !Connected )
                { throw new InvalidOperationException( "Device is not connected" ); }

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

            return _accessor.GetStringAttribute( Path.Combine( _path, attributeName ) );
	    }

		protected string[] GetStringArrayAttribute( string attributeName )
		{
            if ( !Connected )
            { throw new InvalidOperationException( "Device is not connected" ); }

            return _accessor.GetStringArrayAttribute( Path.Combine( _path, attributeName ) );
		}

		protected string[] GetStringSelectorAttribute( string attributeName, out string selected )
		{
            if ( !Connected )
            { throw new InvalidOperationException( "Device is not connected" ); }
			
			return _accessor.GetStringSelectorAttribute( Path.Combine( _path, attributeName ), 
                                                         out selected );
		}

	    protected void SetStringAttribute( string attributeName, string value )
	    {
            if ( !Connected )
            { throw new InvalidOperationException( "Device is not connected" ); }

            _accessor.SetStringAttribute( Path.Combine( _path, attributeName ), value );
        }

	    protected void SetIntAttribute( string attributeName, int value )
	    {
            if ( !Connected )
            { throw new InvalidOperationException( "Device is not connected" ); }

            _accessor.SetIntAttribute( Path.Combine( _path, attributeName ), value );
        }

	    protected int GetIntAttribute( string attributeName )
	    {
            if ( !Connected )
            { throw new InvalidOperationException( "Device is not connected" ); }

            return _accessor.GetIntAttribute( Path.Combine( _path, attributeName ) );
        }

		protected int GetRawData( string attributeName, byte[] buffer, int offset, int count )
		{
			if ( !Connected )
			{ throw new InvalidOperationException( "Device is not connected" ); }

            return _accessor.GetRawData( Path.Combine( _path, attributeName ), 
                                         buffer,
                                         offset,
                                         count );
		}

		protected bool Connect( string classDirectory, 
                                string pattern, 
                                IDictionary<string, string[]> matchCriteria )
	    {
			if ( !Directory.Exists( classDirectory ) )
			{ return false; }

		    var directories = Directory.EnumerateDirectories( classDirectory );

		    foreach ( var directory in directories )
		    {
			    var directoryName = Path.GetFileName( directory );
			    if ( directoryName != null && directoryName.StartsWith( pattern ) )
			    {
				    bool match = true;

				    foreach ( var matchCriterion in matchCriteria )
				    {
					    using ( var attributeStream = new FileStream( $@"{directory}/{matchCriterion.Key}",
                                                                      FileMode.Open, 
                                                                      FileAccess.Read ) )
					    {
						    using ( var reader = new StreamReader( attributeStream ) )
						    {
							    var value = reader.ReadLine( );
							    if ( !matchCriterion.Value.Any( x => value != null && value.Equals( x ) ) )
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

		/// <summary>
		/// Closes all connections to device attributes. 
        /// Next access to the attribute will open the connection again.
		/// </summary>
		public void ResetConnections( )
		{
            _accessor.ResetConnections( );
		}

	    public virtual void Dispose( )
	    {
            _accessor.Dispose( );
	    }

		public const string SysRoot = @"/sys/class";
    }
}
