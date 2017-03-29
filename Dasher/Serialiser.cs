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
using System.IO;

namespace Dasher
{
    /// <summary>
    /// Serialises instances of type <typeparamref name="T"/> as Dasher encoded data.
    /// </summary>
    /// <typeparam name="T">The type to serialise from.</typeparam>
    public sealed class Serialiser<T>
    {
        private readonly Action<Packer, DasherContext, object> _action;
        private readonly DasherContext _context;

        /// <summary>
        /// Initialises an instance of the serialiser.
        /// </summary>
        /// <remarks>
        /// This constructor attempts to retrieve generated serialisation code from <paramref name="context"/>.
        /// If unavailable, it will generate the relevant serialisation code and cache it in <paramref name="context"/> for future use.
        /// </remarks>
        /// <param name="context">An optional context for serialisation. If none is provided, a new context will be created for use by this serialiser alone.</param>
        public Serialiser(DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _action = _context.GetOrCreateSerialiseAction(typeof(T));
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to write serialised data to.</param>
        /// <param name="value">The value to serialise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        public void Serialise(Stream stream, T value)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using (var packer = new Packer(stream))
                Serialise(packer, value);
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to <paramref name="packer"/>.
        /// </summary>
        /// <param name="packer">The packer to pack serialised data to.</param>
        /// <param name="value">The value to serialise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packer"/> is <c>null</c>.</exception>
        public void Serialise(Packer packer, T value)
        {
            if (packer == null)
                throw new ArgumentNullException(nameof(packer));
            _action(packer, _context, value);
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to a byte array.
        /// </summary>
        /// <param name="value">The value to serialise.</param>
        /// <returns>A byte array containing the serialised value.</returns>
        public byte[] Serialise(T value)
        {
            var stream = new MemoryStream();
            using (var packer = new Packer(stream))
                Serialise(packer, value);
            return stream.ToArray();
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to a byte array segment.
        /// </summary>
        /// <remarks>This method can be more efficient than <see cref="Serialise(T)"/> as it needn't trim the buffer.</remarks>
        /// <param name="value">The value to serialise.</param>
        /// <returns>A byte array segment containing the serialised value.</returns>
        public ArraySegment<byte> SerialiseSegment(T value)
        {
            var stream = new MemoryStream();
            using (var packer = new Packer(stream))
                Serialise(packer, value);
#if NET45
            return new ArraySegment<byte>(stream.GetBuffer(), 0, checked((int)stream.Length));
#else
            stream.TryGetBuffer(out var buffer);
            return buffer;
#endif
        }
    }

    /// <summary>
    /// Serialises instances of the type provided to the constructor as Dasher encoded data.
    /// </summary>
    public sealed class Serialiser
    {
        private readonly Action<Packer, DasherContext, object> _action;
        private readonly DasherContext _context;

        /// <summary>
        /// Initialises an instance of the serialiser.
        /// </summary>
        /// <remarks>
        /// This constructor attempts to retrieve generated serialisation code from <paramref name="context"/>.
        /// If unavailable, it will generate the relevant serialisation code and cache it in <paramref name="context"/> for future use.
        /// </remarks>
        /// <param name="type">The type to generate serialisation code for.</param>
        /// <param name="context">An optional context for serialisation. If none is provided, a new context will be created for use by this serialiser alone.</param>
        public Serialiser(Type type, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _action = _context.GetOrCreateSerialiseAction(type);
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to write serialised data to.</param>
        /// <param name="value">The value to serialise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        public void Serialise(Stream stream, object value)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using (var packer = new Packer(stream))
                Serialise(packer, value);
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to <paramref name="packer"/>.
        /// </summary>
        /// <param name="packer">The packer to pack serialised data to.</param>
        /// <param name="value">The value to serialise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packer"/> is <c>null</c>.</exception>
        public void Serialise(Packer packer, object value)
        {
            if (packer == null)
                throw new ArgumentNullException(nameof(packer));
            _action(packer, _context, value);
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to a byte array.
        /// </summary>
        /// <param name="value">The value to serialise.</param>
        /// <returns>A byte array containing the serialised value.</returns>
        public byte[] Serialise(object value)
        {
            var stream = new MemoryStream();
            using (var packer = new Packer(stream))
                Serialise(packer, value);
            return stream.ToArray();
        }

        /// <summary>
        /// Serialises <paramref name="value"/> to a byte array segment.
        /// </summary>
        /// <remarks>This method can be more efficient than <see cref="Serialise(object)"/> as it needn't trim the buffer.</remarks>
        /// <param name="value">The value to serialise.</param>
        /// <returns>A byte array segment containing the serialised value.</returns>
        public ArraySegment<byte> SerialiseSegment(object value)
        {
            var stream = new MemoryStream();
            using (var packer = new Packer(stream))
                Serialise(packer, value);
#if NET45
            return new ArraySegment<byte>(stream.GetBuffer(), 0, checked((int)stream.Length));
#else
            stream.TryGetBuffer(out var buffer);
            return buffer;
#endif

        }
    }
}