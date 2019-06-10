using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.Accessors
{
    public class DebugAccessor<TAccessor> : IAttributeAccessor where TAccessor : IAttributeAccessor, new( )
    {
        private IAttributeAccessor _origin = new TAccessor( );
        private Action<string> _output;

        public DebugAccessor( )
        {
            _output = Console.WriteLine;
        }

        public DebugAccessor( Action<string> output )
        {
            _output = output;
        }

        public void Dispose( )
        {
            _output( $"{typeof( TAccessor ).Name} is disposed" );
            _origin.Dispose( );
        }

        public int GetIntAttribute( string attributePath )
        {
            var value = _origin.GetIntAttribute( attributePath );
            _output( $"Got {value} from {attributePath}" );
            return value;
        }

        public int GetRawData( string attributePath, byte[] buffer, int offset, int count )
        {
            _output( $"Requested {count} bytes of raw data from {attributePath}" );
            return _origin.GetRawData( attributePath, buffer, offset, count );
        }

        private string ArrayToString( string[] array, string selected = null )
        {
            var builder = new StringBuilder( "{ " );

            if (array.Length == 0)
            { return builder.Append( "}" ).ToString( ); }

            foreach ( var str in array )
            {
                if ( str == selected )
                { builder.Append( "[" ).Append( str ).Append( "] " ); }
                else
                { builder.Append( str ).Append( ' ' ); }
            }

            return builder.ToString( );
        }

        public string[] GetStringArrayAttribute( string attributePath )
        {
            var array = _origin.GetStringArrayAttribute( attributePath );
            _output( $"Got {ArrayToString( array )} from {attributePath}" );
            return array;
        }

        public string GetStringAttribute( string attributePath )
        {
            var value = _origin.GetStringAttribute( attributePath );
            _output( $"Got {value} from {attributePath}" );
            return value;
        }

        public string[] GetStringSelectorAttribute( string attributePath, out string selected )
        {
            var array = _origin.GetStringSelectorAttribute( attributePath, out selected );
            _output( $"Got {ArrayToString( array, selected )} from {attributePath}" );
            return array;
        }

        public void ResetConnections( )
        {
            _output( $"{typeof( TAccessor ).Name} reset connections" );
            _origin.ResetConnections( );
        }

        public void SetIntAttribute( string attributePath, int value )
        {
            _output( $"Set value {value} to {attributePath}" );
            _origin.SetIntAttribute( attributePath, value );
        }

        public void SetStringAttribute( string attributePath, string value )
        {
            _output( $"Set value {value} to {attributePath}" );
            _origin.SetStringAttribute( attributePath, value );
        }
    }
}
