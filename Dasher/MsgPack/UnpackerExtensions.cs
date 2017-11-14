#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/dasher
//
#endregion

using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Dasher
{
    /// <summary>
    /// Extension methods for <see cref="Unpacker"/>.
    /// </summary>
    public static class UnpackerExtensions
    {
        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadArrayLength"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="length">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,length:null")]
        [ContractAnnotation("=>true,length:notnull")]
        public static bool TryReadArrayLength(this Unpacker unpacker, out int? length)
        {
            if (unpacker.TryReadArrayLength(out var v))
            {
                length = v;
                return true;
            }
            length = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadMapLength"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="length">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,length:null")]
        [ContractAnnotation("=>true,length:notnull")]
        public static bool TryReadMapLength(this Unpacker unpacker, out int? length)
        {
            if (unpacker.TryReadMapLength(out var v))
            {
                length = v;
                return true;
            }
            length = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadBoolean"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadBoolean(this Unpacker unpacker, out bool? value)
        {
            if (unpacker.TryReadBoolean(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadSByte"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadSByte(this Unpacker unpacker, out sbyte? value)
        {
            if (unpacker.TryReadSByte(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadInt16"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadInt16(this Unpacker unpacker, out short? value)
        {
            if (unpacker.TryReadInt16(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadInt32"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadInt32(this Unpacker unpacker, out int? value)
        {
            if (unpacker.TryReadInt32(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadInt64"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadInt64(this Unpacker unpacker, out long? value)
        {
            if (unpacker.TryReadInt64(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadByte"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadByte(this Unpacker unpacker, out byte? value)
        {
            if (unpacker.TryReadByte(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadUInt16"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadUInt16(this Unpacker unpacker, out ushort? value)
        {
            if (unpacker.TryReadUInt16(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadUInt32"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadUInt32(this Unpacker unpacker, out uint? value)
        {
            if (unpacker.TryReadUInt32(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadUInt64"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadUInt64(this Unpacker unpacker, out ulong? value)
        {
            if (unpacker.TryReadUInt64(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadSingle"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadSingle(this Unpacker unpacker, out float? value)
        {
            if (unpacker.TryReadSingle(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Extension overload of <see cref="Unpacker.TryReadDouble"/> that uses a nullable out parameter.
        /// </summary>
        /// <param name="unpacker">The unpacker to read a value from.</param>
        /// <param name="value">The read value, or <c>null</c> if the value could not be read.</param>
        /// <returns><c>true</c> if the value could be read, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,value:null")]
        [ContractAnnotation("=>true,value:notnull")]
        public static bool TryReadDouble(this Unpacker unpacker, out double? value)
        {
            if (unpacker.TryReadDouble(out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
    }
}
