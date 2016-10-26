using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.Accessors
{
    public class AttributeAccessor : IAttributeAccessor
    {
        private readonly IDictionary<string, StreamWriter> _writableAttributes;

        public AttributeAccessor( )
        {
            _writableAttributes = new Dictionary<string, StreamWriter>( );
        }

        public void Dispose( )
        {
            foreach ( var stream in _writableAttributes )
            {
                stream.Value.Dispose( );
            }
            _writableAttributes.Clear( );
        }

        public int GetIntAttribute( string attributePath )
        {
            return int.Parse( GetStringAttribute( attributePath ) );
        }

        public int GetRawData( string attributePath, byte[] buffer, int offset, int count )
        {
            using ( var stream = new FileStream( attributePath, FileMode.Open, FileAccess.Read ) )
            {
                return stream.Read( buffer, offset, count );
            }
        }

        public string[] GetStringArrayAttribute( string attributePath )
        {
            return GetStringAttribute( attributePath ).Split( );
        }

        public string GetStringAttribute( string attributePath )
        {
            using ( var stream = new FileStream( attributePath,
                                                 FileMode.Open,
                                                 FileAccess.Read,
                                                 FileShare.ReadWrite ) )
            {
                using ( var reader = new StreamReader( stream ) )
                {
                    return reader.ReadToEnd( );
                }
            }
        }

        public string[] GetStringSelectorAttribute( string attributePath, out string selected )
        {
            var variants = GetStringArrayAttribute( attributePath );
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

        public void ResetConnections( )
        {
            foreach ( var stream in _writableAttributes )
            {
                stream.Value.Dispose( );
            }
            _writableAttributes.Clear( );
        }

        public void SetIntAttribute( string attributePath, int value )
        {
            SetStringAttribute( attributePath, value.ToString( ) );
        }

        public void SetStringAttribute( string attributePath, string value )
        {
            StreamWriter writer;
            if ( !_writableAttributes.ContainsKey( attributePath ) )
            {
                var stream = new FileStream( attributePath,
                                             FileMode.Open,
                                             FileAccess.Write,
                                             FileShare.Read );
                writer = new StreamWriter( stream );
                _writableAttributes.Add( attributePath, writer );
            }
            else
            {
                writer = _writableAttributes[attributePath];
            }
            writer.Write( value );
            writer.Flush( );
        }
    }
}
