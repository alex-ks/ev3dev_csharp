﻿using System;
using System.Collections.Generic;

namespace Ev3Dev.CSharp
{
    public class Sensor : Device
    {
        private static readonly IDictionary<string, int> FormatSizes;

        static Sensor()
        {
            FormatSizes = new Dictionary<string, int>
            {
                { U8Format, sizeof(byte) },
                { S8Format, sizeof(sbyte) },
                { U16Format, sizeof(ushort) },
                { S16Format, sizeof(short) },
                { S16BigEndianFormat, sizeof(short) },
                { S32Format, sizeof(int) },
                { S32BigEndianFormat, sizeof(int) },
                { FloatFormat, sizeof(float) }
            };
        }

        public Sensor(string address)
        {
            var success = Connect(new Dictionary<string, string[]>
            {
                { AddressAttribute, new[] { address } }
            });
            if (!success)
                throw new ArgumentException("Sensor is not found");
        }

        public Sensor(string address, string[] sensorTypes)
        {
            var success = Connect(new Dictionary<string, string[]>
            {
                { AddressAttribute, new[] { address } },
                { DriverNameAttribute, sensorTypes }
            });

            if (!success)
            { throw new ArgumentException("Sensor is not found"); }
        }

        protected bool Connect(IDictionary<string, string[]> matchCriteria)
        {
            string path = $@"{SysRoot}/{SensorClass}/";
            return Connect(path, SensorPattern, matchCriteria);
        }

        private volatile byte[] _binaryData;

        /// <summary>
        /// Returns the name of the port that the sensor is connected to, e.g. `ev3:in1`.
        /// I2C sensors also include the I2C address (decimal), e.g. `ev3:in1:i2c8`.
        /// </summary>
        public string Address => GetStringAttribute(AddressAttribute);

        /// <summary>
        /// Reading the file will give the unscaled raw values in the value&lt;N&gt; attributes.
        /// Use `bin_data_format`, `num_values` and the individual sensor documentation to determine how to interpret the data.
        /// </summary>
        public byte[] BinaryData
        {
            get
            {
                int valueCount = NumValues;
                string format = BinaryDataFormat;

                if (!FormatSizes.TryGetValue(format, out int valueSize))
                    throw new InvalidOperationException("Unknown binary data format");

                if (_binaryData == null || _binaryData.Length != valueCount * valueSize)
                    _binaryData = new byte[valueCount * valueSize];

                GetRawData(BinaryDataAttribute, _binaryData, 0, _binaryData.Length);

                return _binaryData;
            }
        }

        /// <summary>
        /// Returns the format of the values in bin_data for the current mode. Possible values are:
        /// <para>u8: Unsigned 8-bit integer (byte)</para>
        /// <para>s8: Signed 8-bit integer (sbyte)</para>
        /// <para>u16: Unsigned 16-bit integer (ushort)</para>
        /// <para>s16: Signed 16-bit integer (short)</para>
        /// <para>s16_be: Signed 16-bit integer, big endian</para>
        /// <para>s32: Signed 32-bit integer (int)</para>
        /// <para>s32_be: Signed 32-bit integer, big endian</para>
        /// <para>float: IEEE 754 32-bit floating point (float)</para>
        /// All types are provided be the class as string constants.
        /// </summary>
        public string BinaryDataFormat => GetStringAttribute(BinaryDataFormatAttribute);

        /// <summary>
        /// Sends a command to the sensor.
        /// </summary>
        public string Command { set { SetStringAttribute(CommandAttribute, value); } }

        /// <summary>
        /// Returns a list of the valid commands for the sensor. Returns -EOPNOTSUPP if no commands are supported.
        /// </summary>
        public string[] Commands => GetStringArrayAttribute(CommandsAttribute);

        /// <summary>
        /// Returns the number of decimal places for the values in the `value&lt;N&gt;` attributes of the current mode.
        /// </summary>
        public int Decimals => GetIntAttribute(DecimalsAttribute);

        /// <summary>
        /// Returns the name of the sensor device/driver. See the list of [supported sensors] for a complete list of drivers.
        /// </summary>
        public string DriverName => GetStringAttribute(DriverNameAttribute);

        /// <summary>
        /// Returns the firmware version of the sensor if available. Currently only NXT/I2C sensors support this.
        /// </summary>
        public string FwVersion => GetStringAttribute(FwVersionAttribute);

        /// <summary>
        /// Returns the current mode. Writing one of the values returned by modes sets the sensor to that mode.
        /// </summary>
        public string Mode
        {
            get { return GetStringAttribute(ModeAttribute); }
            set { SetStringAttribute(ModeAttribute, value); }
        }

        /// <summary>
        /// Returns a list of the valid modes for the sensor.
        /// </summary>
        public string[] Modes => GetStringArrayAttribute(ModesAttribute);

        /// <summary>
        /// Returns the number of value&lt;N&gt; attributes that will return a valid value for the current mode.
        /// </summary>
        public int NumValues => GetIntAttribute(NumValuesAttribute);

        /// <summary>
        /// Returns the units of the measured value for the current mode. May return empty string.
        /// </summary>
        public string Units => GetStringAttribute(UnitsAttribute);

        /// <summary>
        /// Returns a space delimited string representing sensor-specific text values. Returns -EOPNOTSUPP
        /// if a sensor does not support text values.
        /// </summary>
        public string TextValue => GetStringAttribute(TextValueAttribute);

        /// <summary>
        /// Returns the value or values measured by the sensor. Check num_values to see how many values there are.
        /// Values with index greater or equal than NumValues will throw an exception.
        /// The values are fixed point numbers, so check decimals to see if you need to divide to get the actual value.
        /// </summary>
        /// <param name="index">Index of required value in value array.</param>
        /// <returns>Fixed-point value as integer. Check Decimals property to get actual value.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int GetValue(int index = 0)
        {
            if (index < 0 || index >= NumValues)
                throw new IndexOutOfRangeException();
            return GetIntAttribute($"value{index}");
        }

        public const string AddressAttribute = "address";
        public const string BinaryDataAttribute = "bin_data";
        public const string BinaryDataFormatAttribute = "bin_data_format";
        public const string CommandAttribute = "command";
        public const string CommandsAttribute = "commands";
        /// <summary>
        /// Allows direct communication with the sensor for using advanced features that are not otherwise available
        /// through the lego-sensor class. Returns -EOPNOTSUPP if the sensor does not support this. Currently this
        /// only works with I2C sensors. For I2C sensors, use :c:func:`seek()` to set the register to read or write from,
        /// then read or write the number of bytes required.
        ///
        /// This attribute presented only like a name, read the file manually if you want to use it.
        /// </summary>
        public const string DirectAttribute = "direct";
        public const string DecimalsAttribute = "decimals";
        public const string DriverNameAttribute = "driver_name";
        public const string FwVersionAttribute = "fw_version";
        public const string ModeAttribute = "mode";
        public const string ModesAttribute = "modes";
        public const string NumValuesAttribute = "num_values";
        public const string PollMs = "poll_ms";
        public const string UnitsAttribute = "units";
        public const string TextValueAttribute = "text_value";

        /// <summary>
        /// Unsigned 8-bit integer (byte).
        /// </summary>
        public const string U8Format = "u8";
        /// <summary>
        /// Signed 8-bit integer (sbyte).
        /// </summary>
        public const string S8Format = "s8";
        /// <summary>
        /// Unsigned 16-bit integer (ushort).
        /// </summary>
        public const string U16Format = "u16";
        /// <summary>
        /// Signed 16-bit integer (short).
        /// </summary>
        public const string S16Format = "s16";
        /// <summary>
        /// Signed 16-bit integer, big endian.
        /// </summary>
        public const string S16BigEndianFormat = "s16_be";
        /// <summary>
        /// Signed 32-bit integer (int).
        /// </summary>
        public const string S32Format = "s32";
        /// <summary>
        /// Signed 32-bit integer, big endian.
        /// </summary>
        public const string S32BigEndianFormat = "s32_be";
        /// <summary>
        /// IEEE 754 32-bit floating point (float).
        /// </summary>
        public const string FloatFormat = "float";

        private const string SensorClass = "lego-sensor";
        private const string SensorPattern = "sensor";
    }
}
