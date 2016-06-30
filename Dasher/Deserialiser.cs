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
using System.IO;

namespace Dasher
{
    public sealed class Deserialiser<T>
    {
        private readonly Func<Unpacker, DasherContext, object> _func;
        private readonly DasherContext _context;

        public Deserialiser(UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _func = _context.GetOrCreateDeserialiser(typeof(T), unexpectedFieldBehaviour);
        }

        public T Deserialise(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            return Deserialise(new Unpacker(new MemoryStream(bytes)));
        }

        public T Deserialise(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialise(new Unpacker(stream));
        }

        public T Deserialise(Unpacker unpacker)
        {
            if (unpacker == null)
                throw new ArgumentNullException(nameof(unpacker));
            return (T)_func(unpacker, _context);
        }
    }

    public sealed class Deserialiser
    {
        private readonly Func<Unpacker, DasherContext, object> _func;
        private readonly DasherContext _context;

        public Deserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _func = _context.GetOrCreateDeserialiser(type, unexpectedFieldBehaviour);
        }

        public object Deserialise(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            return Deserialise(new Unpacker(new MemoryStream(bytes)));
        }

        public object Deserialise(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialise(new Unpacker(stream));
        }

        public object Deserialise(Unpacker unpacker)
        {
            if (unpacker == null)
                throw new ArgumentNullException(nameof(unpacker));
            return _func(unpacker, _context);
        }
    }
}