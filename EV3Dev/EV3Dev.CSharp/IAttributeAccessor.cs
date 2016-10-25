using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp
{
    public interface IAttributeAccessor : IDisposable
    {
        string GetStringAttribute( string attributePath );
        string[] GetStringArrayAttribute( string attributePath );
        string[] GetStringSelectorAttribute( string attributePath, out string selected );
        void SetStringAttribute( string attributePath, string value );

        int GetIntAttribute( string attributePath );
        void SetIntAttribute( string attributePath, int value );

        int GetRawData( string attributePath, byte[] buffer, int offset, int count );

        void ResetConnections( );
    }
}
