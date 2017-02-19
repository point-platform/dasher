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
using System.Diagnostics.CodeAnalysis;

namespace Dasher
{
    /// <summary>
    /// Commonly used to signify an empty message.
    /// </summary>
    /// <remarks>
    /// <see cref="Empty"/> is mainly used with generic wrapper types. Consider
    /// a message envelope such as <c>Envelope&lt;T&gt;</c> for which header
    /// fields apply, but no body is required. In such a case you can serialise
    /// and deserialise <c>Envelope&lt;Empty&gt;</c> successfully.
    /// <para />
    /// On the wire, <c>Empty</c> is serialised as a MsgPack map with zero elements.
    /// Conceptually this is a complex type with no fields.
    /// <para />
    /// A serialised <c>Empty</c> value can be deserialised as a complex type if
    /// all parameters have default values, allowing versioning of contracts over time.
    /// <para />
    /// This class may not be instantiated.
    /// </remarks>
    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    public sealed class Empty
    {
        private Empty()
        {
            throw new NotSupportedException("Not for instantiation.");
        }
    }
}