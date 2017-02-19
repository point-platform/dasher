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
using Dasher.TypeProviders;

namespace Dasher
{
    /// <summary>
    /// Specifies the tag used to identify union members.
    /// </summary>
    /// <remarks>
    /// This attribute may be placed on types used in generic <c>Union</c> type parameters
    /// as a way of explicitly specifying the string identifier used to tag the specific
    /// union member type on the wire.
    /// <para />
    /// This attribute is optional.
    /// If not present on a union member type, a tag will be assigned via <see cref="UnionEncoding.GetTypeName"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class UnionTagAttribute : Attribute
    {
        /// <summary>
        /// The tag used to identify the union member type.
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// Initialises an instance of this attribute with a given identifier.
        /// </summary>
        /// <param name="tag">The tag used to identify the union member type.</param>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c> or white space.</exception>
        public UnionTagAttribute(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Cannot be null or white space.", nameof(tag));

            Tag = tag;
        }
    }
}