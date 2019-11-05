using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Ev3Dev.CSharp
{
    public abstract class Device : IDisposable
    {
        private string _path;
        private int _deviceIndex = -1;

        private readonly Dictionary<string, StreamWriter> _writableAttributes;

        protected Device()
        {
            _writableAttributes = new Dictionary<string, StreamWriter>();
        }

        public bool Connected => !string.IsNullOrEmpty(_path);

        public int DeviceIndex
        {
            get
            {
                if (!Connected)
                    throw new InvalidOperationException("Device is not connected");

                if (_deviceIndex < 0)
                {
                    int rank = 1;
                    _deviceIndex = 0;
                    foreach (var c in _path.Where(char.IsDigit))
                    {
                        _deviceIndex += (c - '0') * rank;
                        rank *= 10;
                    }
                }

                return _deviceIndex;
            }
        }

        protected int GetIntAttribute(string attributeName)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");
            var attributePath = Path.Combine(_path, attributeName);
            return int.Parse(GetStringAttribute(attributePath));
        }

        protected string GetStringAttribute(string attributeName)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");

            var attributePath = Path.Combine(_path, attributeName);

            using (var stream = new FileStream(attributePath,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        protected string[] GetStringArrayAttribute(string attributeName)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");
            return GetStringAttribute(attributeName).Split();
        }

        protected string[] GetStringSelectorAttribute(string attributeName, out string selected)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");

            var variants = GetStringArrayAttribute(attributeName);
            selected = null;

            for (int i = 0; i < variants.Length; ++i)
            {
                if (variants[i].StartsWith("[") && variants[i].EndsWith("]"))
                {
                    selected = variants[i].Substring(1, variants[i].Length - 2);
                    variants[i] = selected;
                    break;
                }
            }

            return variants;
        }

        protected void SetStringAttribute(string attributeName, string value)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");

            var attributePath = Path.Combine(_path, attributeName);

            StreamWriter writer;
            if (!_writableAttributes.ContainsKey(attributePath))
            {
                var stream = new FileStream(attributePath,
                                            FileMode.Open,
                                            FileAccess.Write,
                                            FileShare.Read);
                writer = new StreamWriter(stream);
                _writableAttributes.Add(attributePath, writer);
            }
            else
            {
                writer = _writableAttributes[attributePath];
            }
            writer.Write(value);
            writer.Flush();
        }

        protected void SetIntAttribute(string attributeName, int value)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");
            SetStringAttribute(Path.Combine(_path, attributeName), value.ToString());
        }

        protected int GetRawData(string attributeName, byte[] buffer, int offset, int count)
        {
            if (!Connected)
                throw new InvalidOperationException("Device is not connected");
            using (var stream = new FileStream(Path.Combine(_path, attributeName), FileMode.Open, FileAccess.Read))
            {
                return stream.Read(buffer, offset, count);
            }
        }

        protected bool Connect(string classDirectory,
                               string pattern,
                               IDictionary<string, string[]> matchCriteria)
        {
            if (!Directory.Exists(classDirectory))
                return false;

            var directories = Directory.EnumerateDirectories(classDirectory);

            foreach (var directory in directories)
            {
                var directoryName = Path.GetFileName(directory);
                if (directoryName != null && directoryName.StartsWith(pattern))
                {
                    bool match = true;

                    foreach (var matchCriterion in matchCriteria)
                    {
                        using (var attributeStream = new FileStream($@"{directory}/{matchCriterion.Key}",
                                                                    FileMode.Open,
                                                                    FileAccess.Read))
                        {
                            using (var reader = new StreamReader(attributeStream))
                            {
                                var value = reader.ReadLine();
                                if (!matchCriterion.Value.Any(x => value != null && value.Equals(x)))
                                {
                                    match = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (match)
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
        public void ResetConnections()
        {
            foreach (var stream in _writableAttributes)
            {
                stream.Value.Dispose();
            }
            _writableAttributes.Clear();
        }

        public virtual void Dispose()
        {
            ResetConnections();
        }

        public const string SysRoot = @"/sys/class";
    }
}
