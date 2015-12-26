using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class MsgPackTypeProvider : ITypeProvider
    {
        private static readonly Dictionary<Type, MethodInfo> _unpackerTryReadMethodByType = new Dictionary<Type, MethodInfo>
        {
            {typeof(sbyte),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSByte))},
            {typeof(byte),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadByte))},
            {typeof(short),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt16))},
            {typeof(ushort), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt16))},
            {typeof(int),    typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt32))},
            {typeof(uint),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt32))},
            {typeof(long),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64))},
            {typeof(ulong),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt64))},
            {typeof(float),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSingle))},
            {typeof(double), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadDouble))},
            {typeof(bool),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBoolean))},
            {typeof(string), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] {typeof(string).MakeByRefType()})},
            {typeof(byte[]), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBinary))}
        };

        public bool CanProvide(Type type) => _unpackerTryReadMethodByType.ContainsKey(type);

        public void Serialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, DasherContext context)
        {
            var packerMethod = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] { value.LocalType });

            if (packerMethod == null)
                throw new InvalidOperationException("Type not supported. Call CanProvide first.");

            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Call, packerMethod);
        }

        public void Deserialise(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            MethodInfo unpackerMethod;
            if (!_unpackerTryReadMethodByType.TryGetValue(value.LocalType, out unpackerMethod))
                throw new InvalidOperationException("Type not supported. Call CanProvide first.");

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, unpackerMethod);

            // If the unpacker method failed (returned false), throw
            var typeGetterSuccess = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, typeGetterSuccess);
            {
                var format = ilg.DeclareLocal(typeof(Format));
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldloca, format);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryPeekFormat)));
                ilg.Emit(OpCodes.Pop);

                ilg.Emit(OpCodes.Ldstr, "Unexpected type for \"{0}\". Expected {1}, got {2}.");
                ilg.Emit(OpCodes.Ldstr, name);
                ilg.Emit(OpCodes.Ldstr, value.LocalType.Name);
                ilg.Emit(OpCodes.Ldloc, format);
                ilg.Emit(OpCodes.Box, typeof(Format));
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object), typeof(object) }));
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(typeGetterSuccess);
        }
    }
}