using System;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class IntPtrProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(IntPtr);

        public void Serialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer)
        {
            // write the int64 form of the value
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(IntPtr).GetMethod(nameof(IntPtr.ToInt64)));
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] { typeof(long) }));
        }

        public void Deserialise(ILGenerator ilg, LocalBuilder value, LocalBuilder unpacker, string name, Type targetType)
        {
            // Read value as a long
            var num = ilg.DeclareLocal(typeof(long));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, num);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64)));

            // If the unpacker method failed (returned false), throw
            var lbl = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int64 value for IntPtr property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl);

            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Ldloc, num);
            ilg.Emit(OpCodes.Call, typeof(IntPtr).GetConstructor(new[] { typeof(long) }));
        }
    }
}