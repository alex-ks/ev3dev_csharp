using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.Accessors
{
    public class SynchronizedAccessor : IAttributeAccessor
    {
        private readonly object dictionaryGuard = new object( );
        private readonly ConcurrentDictionary<string, object> _attributeLocks;
        private readonly AttributeAccessor _accessor;

        public SynchronizedAccessor( )
        {
            _attributeLocks = new ConcurrentDictionary<string, object>( );
            _accessor = new AttributeAccessor( );
        }

        public void Dispose( )
        {
            lock ( dictionaryGuard )
            {
                foreach ( var attributeLock in _attributeLocks )
                {
                    Monitor.Enter( attributeLock.Value );
                }

                _accessor.Dispose( );

                foreach ( var attributeLock in _attributeLocks )
                {
                    Monitor.Exit( attributeLock.Value );
                }

                _attributeLocks.Clear( );
            }
        }

        private T LockMethod<T>( string attributePath, Func<string, T> method )
        {
            object lockGuard;
            lock ( dictionaryGuard )
            {
                lockGuard = _attributeLocks.GetOrAdd( attributePath, new object( ) );
            }

            lock ( lockGuard )
            {
                return method( attributePath );
            }
        }

        private void LockMethod( string attributePath, Action<string> method )
        {
            object lockGuard;
            lock ( dictionaryGuard )
            {
                lockGuard = _attributeLocks.GetOrAdd( attributePath, new object( ) );
            }

            lock ( lockGuard )
            {
                method( attributePath );
            }
        }

        public int GetIntAttribute( string attributePath )
        {
            return LockMethod( attributePath, _accessor.GetIntAttribute );
        }

        public int GetRawData( string attributePath, byte[] buffer, int offset, int count )
        {
            return LockMethod( attributePath, 
                               ( path ) => _accessor.GetRawData( path, buffer, offset, count ) );
        }

        public string[] GetStringArrayAttribute( string attributePath )
        {
            return LockMethod( attributePath, _accessor.GetStringArrayAttribute );
        }

        public string GetStringAttribute( string attributePath )
        {
            return LockMethod( attributePath, _accessor.GetStringAttribute );
        }

        public string[] GetStringSelectorAttribute( string attributePath, out string selected )
        {
            object lockGuard;
            lock ( dictionaryGuard )
            {
                lockGuard = _attributeLocks.GetOrAdd( attributePath, new object( ) );
            }

            lock ( lockGuard )
            {
                return _accessor.GetStringSelectorAttribute( attributePath, out selected );
            }
        }

        public void ResetConnections( )
        {
            lock ( dictionaryGuard )
            {
                foreach ( var attributeLock in _attributeLocks )
                {
                    Monitor.Enter( attributeLock.Value );
                }

                _accessor.ResetConnections( );

                foreach ( var attributeLock in _attributeLocks )
                {
                    Monitor.Exit( attributeLock.Value );
                }
            }
        }

        public void SetIntAttribute( string attributePath, int value )
        {
            LockMethod( attributePath, ( path ) => _accessor.SetIntAttribute( path, value ) );
        }

        public void SetStringAttribute( string attributePath, string value )
        {
            LockMethod( attributePath, ( path ) => _accessor.SetStringAttribute( path, value ) );
        }
    }
}
