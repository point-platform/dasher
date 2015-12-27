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

        public Serialiser()
        {
            _inner = new Serialiser(typeof(T));
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
        private readonly Action<UnsafePacker, object> _action;

        public Serialiser(Type type, DasherContext context = null)
        {
            _action = BuildPacker(type, context ?? new DasherContext());
        }

        public void Serialise(Stream stream, object value)
        {
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
        }

        public void Serialise(UnsafePacker packer, object value)
        {
            _action(packer, value);
        }

        public byte[] Serialise(object value)
        {
            var stream = new MemoryStream();
            using (var packer = new UnsafePacker(stream))
                Serialise(packer, value);
            return stream.ToArray();
        }

        private static Action<UnsafePacker, object> BuildPacker(Type type, DasherContext context)
        {
            if (type.IsPrimitive)
                throw new Exception("TEST THIS CASE 1");

            var method = new DynamicMethod(
                $"Serialiser{type.Name}",
                null,
                new[] { typeof(UnsafePacker), typeof(object) });

            var ilg = method.GetILGenerator();

            // store packer in a local so we can pass it easily
            var packer = ilg.DeclareLocal(typeof(UnsafePacker));
            ilg.Emit(OpCodes.Ldarg_0); // packer
            ilg.Emit(OpCodes.Stloc, packer);

            // cast value to a local of required type
            var value = ilg.DeclareLocal(type);
            ilg.Emit(OpCodes.Ldarg_1); // value
            ilg.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            ilg.Emit(OpCodes.Stloc, value);

            ITypeProvider provider;
            if (!context.TryGetTypeProvider(value.LocalType, out provider))
                throw new Exception($"Cannot serialise type {value.LocalType}.");

            provider.Serialise(ilg, value, packer, context);

            ilg.Emit(OpCodes.Ret);

            // Return a delegate that performs the above operations
            return (Action<UnsafePacker, object>)method.CreateDelegate(typeof(Action<UnsafePacker, object>));
        }
    }
}