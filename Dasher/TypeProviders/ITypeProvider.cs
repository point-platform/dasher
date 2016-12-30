#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
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
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    /// <summary>
    /// Generate serialisation and deserialisation code for one or more .NET types.
    /// </summary>
    /// <remarks>
    /// Dasher includes implementations that cover most core built-in .NET types.
    /// Other types may be supported by registering implementations of this interface
    /// with a <see cref="DasherContext"/> and providing that context to serialisers and
    /// deserialisers.
    /// </remarks>
    public interface ITypeProvider
    {
        /// <summary>
        /// Get whether this provider supports <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns><c>true</c> if this provider supports <paramref name="type"/>, otherwise <c>false</c>.</returns>
        bool CanProvide(Type type);

        /// <summary>
        /// Get whether this provider uses default <c>null</c> handling.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns><c>true</c> if this provider uses default <c>null</c> handling, otherwise <c>false</c>.</returns>
        bool UseDefaultNullHandling(Type type);

        /// <summary>
        /// Attempt to generate serialisation code.
        /// </summary>
        /// <param name="ilg">The IL generator to emit code to.</param>
        /// <param name="throwBlocks">Used to move exception-generating code out of line of happy-path code.</param>
        /// <param name="errors">A collection to which error message will be registered.</param>
        /// <param name="value">The local variable holding the value to serialise.</param>
        /// <param name="packer">The local variable holding the packer to pack MsgPack values to.</param>
        /// <param name="contextLocal">The local variable holding the Dasher context.</param>
        /// <param name="context">The Dasher context.</param>
        /// <returns><c>true</c> if code generation succeeded, otherwise <c>false</c>.</returns>
        bool TryEmitSerialiseCode(
            ILGenerator ilg,
            ThrowBlockGatherer throwBlocks,
            ICollection<string> errors,
            LocalBuilder value,
            LocalBuilder packer,
            LocalBuilder contextLocal,
            DasherContext context);

        /// <summary>
        /// Attempt to generate serialisation code.
        /// </summary>
        /// <param name="ilg">The IL generator to emit code to.</param>
        /// <param name="throwBlocks">Used to move exception-generating code out of line of happy-path code.</param>
        /// <param name="errors">A collection to which error message will be registered.</param>
        /// <param name="name">Name of the property being deserialised.</param>
        /// <param name="targetType">Type of the top-level object being deserialised.</param>
        /// <param name="value">The local variable holding the value to serialise.</param>
        /// <param name="unpacker">The local variable holding the unpacker to unpack MsgPack values from.</param>
        /// <param name="contextLocal">The local variable holding the Dasher context.</param>
        /// <param name="context">The Dasher context.</param>
        /// <param name="unexpectedFieldBehaviour">Controls how deserialisation behaves when unexpected data is received. Default is <see cref="UnexpectedFieldBehaviour.Throw"/>.</param>
        /// <returns><c>true</c> if code generation succeeded, otherwise <c>false</c>.</returns>
        bool TryEmitDeserialiseCode(
            ILGenerator ilg,
            ThrowBlockGatherer throwBlocks,
            ICollection<string> errors,
            string name,
            Type targetType,
            LocalBuilder value,
            LocalBuilder unpacker,
            LocalBuilder contextLocal,
            DasherContext context,
            UnexpectedFieldBehaviour unexpectedFieldBehaviour);
    }
}