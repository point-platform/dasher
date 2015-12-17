using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MsgPack.Strict
{
    public sealed class StrictSerialiser<T>
    {
        private readonly StrictSerialiser _inner;

        internal StrictSerialiser(StrictSerialiser inner)
        {
            _inner = inner;
        }

        public void Serialise(Stream stream, T value)
        {
            _inner.Serialise(stream, value);
        }

        public void Serialise(UnsafeMsgPackPacker packer, T value)
        {
            _inner.Serialise(packer, value);
        }

        public byte[] Serialise(T value)
        {
            return _inner.Serialise(value);
        }
    }

    public sealed class StrictSerialiser
    {
        #region Instance accessors

        private static readonly ConcurrentDictionary<Type, StrictSerialiser> _serialiserByType = new ConcurrentDictionary<Type, StrictSerialiser>();

        public static StrictSerialiser<T> Get<T>()
        {
            return new StrictSerialiser<T>(Get(typeof(T)));
        }

        public static StrictSerialiser Get(Type type)
        {
            StrictSerialiser deserialiser;
            if (_serialiserByType.TryGetValue(type, out deserialiser))
                return deserialiser;

            _serialiserByType.TryAdd(type, new StrictSerialiser(type));
            var present = _serialiserByType.TryGetValue(type, out deserialiser);
            Debug.Assert(present);
            return deserialiser;
        }

        #endregion

        private readonly Action<UnsafeMsgPackPacker, object> _action;

        private StrictSerialiser(Type type)
        {
            _action = BuildPacker(type);
        }

        public void Serialise(Stream stream, object value)
        {
            using (var packer = new UnsafeMsgPackPacker(stream))
                Serialise(packer, value);
        }

        public void Serialise(UnsafeMsgPackPacker packer, object value)
        {
            _action(packer, value);
        }

        public byte[] Serialise(object value)
        {
            var stream = new MemoryStream();
            using (var packer = new UnsafeMsgPackPacker(stream))
                _action(packer, value);
            return stream.ToArray();
        }

        private static Action<UnsafeMsgPackPacker, object> BuildPacker(Type type)
        {
            if (type.IsPrimitive)
                throw new Exception("TEST THIS CASE 1");

            var method = new DynamicMethod(
                $"Serialiser{type.Name}",
                null,
                new[] { typeof(UnsafeMsgPackPacker), typeof(object) });

            var ilg = method.GetILGenerator();

            var props = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToList();

            // store packer in a local so we can pass it easily
            var packer = ilg.DeclareLocal(typeof(UnsafeMsgPackPacker));
            ilg.Emit(OpCodes.Ldarg_0); // packer
            ilg.Emit(OpCodes.Stloc, packer);

            // cast value to a local of required type
            var value = ilg.DeclareLocal(type);
            ilg.Emit(OpCodes.Ldarg_1); // value
            ilg.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            ilg.Emit(OpCodes.Stloc, value);

            // write map header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldc_I4, props.Count);
            ilg.Emit(OpCodes.Call, typeof(UnsafeMsgPackPacker).GetMethod(nameof(UnsafeMsgPackPacker.PackMapHeader)));

            // write each property's value
            foreach (var prop in props)
            {
                var propValue = ilg.DeclareLocal(prop.PropertyType);

                // write property name
                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldstr, prop.Name);
                ilg.Emit(OpCodes.Call, typeof(UnsafeMsgPackPacker).GetMethod(nameof(UnsafeMsgPackPacker.Pack), new[] {typeof(string)}));

                // get property value
                ilg.Emit(type.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Call, prop.GetMethod);
                ilg.Emit(OpCodes.Stloc, propValue);

                // write property value
                WriteValue(ilg, packer, propValue);
            }

            ilg.Emit(OpCodes.Ret);

            // Return a delegate that performs the above operations
            return (Action<UnsafeMsgPackPacker, object>)method.CreateDelegate(typeof(Action<UnsafeMsgPackPacker, object>));
        }

        private static void WriteValue(ILGenerator ilg, LocalBuilder packer, LocalBuilder value)
        {
            var type = value.LocalType;

            var packerMethod = typeof(UnsafeMsgPackPacker).GetMethod(nameof(UnsafeMsgPackPacker.Pack), new[] {type});
            if (packerMethod != null)
            {
                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Call, packerMethod);
                return;
            }

            throw new NotSupportedException($"Cannot serialise property of type {type}");
        }
    }
}