#region License
//
// Dasher
//
// Copyright 2015 Drew Noakes
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
using System.Reflection.Emit;
using Dasher.TypeProviders;

namespace Dasher
{
    public sealed class Serialiser<T>
    {
        private readonly Serialiser _inner;

        public Serialiser(DasherContext context = null)
        {
            _inner = new Serialiser(typeof(T), context);
        }

        public void Serialise(Stream stream, T value)
        {
            _inner.Serialise(stream, value);
        }

        public void Serialise(UnsafePacker packer, T value)
        {
            _inner.Serialise(packer, value);
        }

        public byte[] Serialise(T value)
        {
            return _inner.Serialise(value);
        }
    }

    public sealed class Serialiser
    {
        private readonly Action<UnsafePacker, DasherContext, object> _action;
        private readonly DasherContext _context;

        public Serialiser(Type type, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _context.RegisterSerialiser(type, this);
            _action = BuildPacker(type, _context);
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

        private static Action<UnsafePacker, DasherContext, object> BuildPacker(Type type, DasherContext context)
        {
            if (type.IsPrimitive)
                throw new Exception("TEST THIS CASE 1");

            var method = new DynamicMethod(
                $"Serialiser{type.Name}",
                null,
                new[] { typeof(UnsafePacker), typeof(DasherContext), typeof(object) },
                restrictedSkipVisibility: true);

            var ilg = method.GetILGenerator();

            // store packer in a local so we can pass it easily
            var packer = ilg.DeclareLocal(typeof(UnsafePacker));
            ilg.Emit(OpCodes.Ldarg_0); // packer
            ilg.Emit(OpCodes.Stloc, packer);

            // store context in a local so we can pass it easily
            var contextLocal = ilg.DeclareLocal(typeof(DasherContext));
            ilg.Emit(OpCodes.Ldarg_1); // context
            ilg.Emit(OpCodes.Stloc, contextLocal);

            // cast value to a local of required type
            var value = ilg.DeclareLocal(type);
            ilg.Emit(OpCodes.Ldarg_2); // value
            ilg.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            ilg.Emit(OpCodes.Stloc, value);

            if (!context.TrySerialise(ilg, value, packer, contextLocal))
                throw new Exception($"Cannot serialise type {value.LocalType}.");

            ilg.Emit(OpCodes.Ret);

            // Return a delegate that performs the above operations
            return (Action<UnsafePacker, DasherContext, object>)method.CreateDelegate(typeof(Action<UnsafePacker, DasherContext, object>));
        }
    }
}