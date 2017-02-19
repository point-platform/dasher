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
    /// Deserialises Dasher encoded data to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialise to.</typeparam>
    public sealed class Deserialiser<T>
    {
        private readonly Func<Unpacker, DasherContext, object> _func;
        private readonly DasherContext _context;

        /// <summary>
        /// Initialises an instance of the deserialiser.
        /// </summary>
        /// <remarks>
        /// This constructor attempts to retrieve generated deserialisation code from <paramref name="context"/>.
        /// If unavailable, it will generate the relevant deserialisation code and cache it in <paramref name="context"/> for future use.
        /// </remarks>
        /// <param name="unexpectedFieldBehaviour">Controls how deserialisation behaves when unexpected data is received. Default is <see cref="UnexpectedFieldBehaviour.Throw"/>.</param>
        /// <param name="context">An optional context for deserialisation. If none is provided, a new context will be created for use by this deserialiser alone.</param>
        public Deserialiser(UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _func = _context.GetOrCreateDeserialiseFunc(typeof(T), unexpectedFieldBehaviour);
        }

        /// <summary>
        /// Deserialises an object from <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The byte array to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        public T Deserialise(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            return Deserialise(new Unpacker(new MemoryStream(bytes)));
        }

        /// <summary>
        /// Deserialises an object from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        public T Deserialise(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialise(new Unpacker(stream));
        }

        /// <summary>
        /// Deserialises an object from <paramref name="unpacker"/>.
        /// </summary>
        /// <param name="unpacker">The unpacker to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unpacker"/> is <c>null</c>.</exception>
        public T Deserialise(Unpacker unpacker)
        {
            if (unpacker == null)
                throw new ArgumentNullException(nameof(unpacker));
            return (T)_func(unpacker, _context);
        }
    }

    /// <summary>
    /// Deserialises Dasher encoded data to the type provided to the constructor.
    /// </summary>
    public sealed class Deserialiser
    {
        private readonly Func<Unpacker, DasherContext, object> _func;
        private readonly DasherContext _context;

        /// <summary>
        /// Initialises an instance of the deserialiser.
        /// </summary>
        /// <remarks>
        /// This constructor attempts to retrieve generated deserialisation code from <paramref name="context"/>.
        /// If unavailable, it will generate the relevant deserialisation code and cache it in <paramref name="context"/> for future use.
        /// </remarks>
        /// <param name="type">The type to generate deserialisation code for.</param>
        /// <param name="unexpectedFieldBehaviour">Controls how deserialisation behaves when unexpected data is received. Default is <see cref="UnexpectedFieldBehaviour.Throw"/>.</param>
        /// <param name="context">An optional context for deserialisation. If none is provided, a new context will be created for use by this deserialiser alone.</param>
        public Deserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _func = _context.GetOrCreateDeserialiseFunc(type, unexpectedFieldBehaviour);
        }

        /// <summary>
        /// Deserialises an object from <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The byte array to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        public object Deserialise(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            return Deserialise(new Unpacker(new MemoryStream(bytes)));
        }

        /// <summary>
        /// Deserialises an object from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        public object Deserialise(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialise(new Unpacker(stream));
        }

        /// <summary>
        /// Deserialises an object from <paramref name="unpacker"/>.
        /// </summary>
        /// <param name="unpacker">The unpacker to deserialise from.</param>
        /// <returns>The deserialised object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unpacker"/> is <c>null</c>.</exception>
        public object Deserialise(Unpacker unpacker)
        {
            if (unpacker == null)
                throw new ArgumentNullException(nameof(unpacker));
            return _func(unpacker, _context);
        }
    }
}