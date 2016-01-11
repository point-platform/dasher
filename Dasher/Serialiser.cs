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
    public sealed class Serialiser<T>
    {
        private readonly Action<UnsafePacker, DasherContext, object> _action;
        private readonly DasherContext _context;

        public Serialiser(DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _action = _context.GetOrCreateSerialiser(typeof(T));
        }

        public void Serialise(Stream stream, T value)
        {
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
        }

        public void Serialise(UnsafePacker packer, T value)
        {
            _action(packer, _context, value);
        }

        public byte[] Serialise(T value)
        {
            var stream = new MemoryStream();
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
            return stream.ToArray();
        }
    }

    public sealed class Serialiser
    {
        private readonly Action<UnsafePacker, DasherContext, object> _action;
        private readonly DasherContext _context;

        public Serialiser(Type type, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _action = _context.GetOrCreateSerialiser(type);
        }

        public void Serialise(Stream stream, object value)
        {
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
        }

        public void Serialise(UnsafePacker packer, object value)
        {
            _action(packer, _context, value);
        }

        public byte[] Serialise(object value)
        {
            var stream = new MemoryStream();
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
            return stream.ToArray();
        }
    }
}