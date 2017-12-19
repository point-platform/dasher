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

using System;
using System.Collections.Generic;
using System.Linq;
using Dasher.Utils;

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when reading or writing the primitive values supported by Dasher.
    /// </summary>
    /// <remarks>
    /// Supported types are:
    /// <list type="bullet">
    ///   <item><see cref="byte" /></item>
    ///   <item><see cref="sbyte" /></item>
    ///   <item><see cref="short" /></item>
    ///   <item><see cref="ushort" /></item>
    ///   <item><see cref="int" /></item>
    ///   <item><see cref="uint" /></item>
    ///   <item><see cref="long" /></item>
    ///   <item><see cref="ulong" /></item>
    ///   <item><see cref="float" /></item>
    ///   <item><see cref="double" /></item>
    ///   <item><see cref="bool" /></item>
    ///   <item><see cref="char" /></item>
    ///   <item><see cref="string" /></item>
    ///   <item><see cref="byte" /> array</item>
    ///   <item><see cref="ArraySegment{T}" /> of byte</item>
    ///   <item><see cref="decimal" /></item>
    ///   <item><see cref="DateTime" /></item>
    ///   <item><see cref="DateTimeOffset" /></item>
    ///   <item><see cref="TimeSpan" /></item>
    ///   <item><see cref="IntPtr" /></item>
    ///   <item><see cref="Guid" /></item>
    ///   <item><see cref="Version" /></item>
    /// </list>
    /// </remarks>
    public sealed class PrimitiveContract : ByValueContract, IWriteContract, IReadContract
    {
        private static readonly Dictionary<Type, string> _nameByType;
        private static readonly Dictionary<string, Type> _typeByName;

        static PrimitiveContract()
        {
            _nameByType = new Dictionary<Type, string>
            {
                {typeof(byte), "Byte"},
                {typeof(sbyte), "SByte"},
                {typeof(short), "Int16"},
                {typeof(ushort), "UInt16"},
                {typeof(int), "Int32"},
                {typeof(uint), "UInt32"},
                {typeof(long), "Int64"},
                {typeof(ulong), "UInt64"},
                {typeof(float), "Single"},
                {typeof(double), "Double"},
                {typeof(bool), "Boolean"},
                {typeof(char), "Char"},
                {typeof(string), "String"},
                {typeof(byte[]), "ByteArray"},
                {typeof(ArraySegment<byte>), "ByteArraySegment"},
                {typeof(decimal), "Decimal"},
                {typeof(DateTime), "DateTime"},
                {typeof(DateTimeOffset), "DateTimeOffset"},
                {typeof(TimeSpan), "TimeSpan"},
                {typeof(IntPtr), "IntPtr"},
                {typeof(Guid), "Guid"},
                {typeof(Version), "Version"}
            };

            _typeByName = _nameByType.ToDictionary(p => p.Value, p => p.Key);
        }

        internal static bool CanProcess(Type type) => _nameByType.ContainsKey(type);

        /// <summary>
        /// Gets the name of the primitive type.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the .NET type that maps to the primitive type.
        /// </summary>
        public Type Type { get; }

        internal PrimitiveContract(Type type)
        {
            if (!_nameByType.TryGetValue(type, out var name))
                throw new ArgumentException($"Type {type} is not a supported primitive.", nameof(type));
            TypeName = name;
            Type = type;
        }

        internal PrimitiveContract(string typeName)
        {
            if (!_typeByName.TryGetValue(typeName, out var type))
                throw new ContractParseException($"Invalid primitive contract name \"{typeName}\".");
            TypeName = typeName;
            Type = type;
        }

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict) => Equals(writeContract);

        /// <inheritdoc />
        public override bool Equals(Contract other) => other is PrimitiveContract contract && contract.TypeName == TypeName;

        /// <inheritdoc />
        protected override int ComputeHashCode() => TypeName.GetHashCode();

        /// <inheritdoc />
        public override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        /// <inheritdoc />
        public override string MarkupValue => TypeName;

        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}
