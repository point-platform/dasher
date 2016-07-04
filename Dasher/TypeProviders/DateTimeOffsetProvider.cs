using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class DateTimeOffsetProvider : ITypeProvider
    {
        private const int TicksPerMinute = 600000000;

        public bool CanProvide(Type type) => type == typeof(DateTimeOffset);

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // We need to write both the date and the offset
            // - dto.DateTime always has unspecified kind (so we can just use Ticks rather than ToBinary and ignore internal flags)
            // - dto.Offset is a timespan but always has integral minutes (minutes will be a smaller number than ticks so uses fewer bytes on the wire)

            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Dup);
            ilg.Emit(OpCodes.Dup);

            // Write the array header
            ilg.Emit(OpCodes.Ldc_I4_2);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackArrayHeader)));

            // Write ticks
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Ticks)).GetMethod);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(long)}));

            // Write offset minutes
            var offset = ilg.DeclareLocal(typeof(TimeSpan));
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Offset)).GetMethod);
            ilg.Emit(OpCodes.Stloc, offset);
            ilg.Emit(OpCodes.Ldloca, offset);
            ilg.Emit(OpCodes.Call, typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)).GetMethod);
            ilg.Emit(OpCodes.Ldc_I4, TicksPerMinute);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Div);
            ilg.Emit(OpCodes.Conv_I2);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] { typeof(short) }));
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Ensure we have an array of two values
            var arrayLength = ilg.DeclareLocal(typeof(int));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, arrayLength);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength)));

            // If the unpacker method failed (returned false), throw
            var lbl1 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl1);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting array header for DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl1);

            ilg.Emit(OpCodes.Ldloc, arrayLength);
            ilg.Emit(OpCodes.Ldc_I4_2);
            ilg.Emit(OpCodes.Ceq);

            var lbl2 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl2);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting array to contain two items for DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl2);

            // Read ticks
            var ticks = ilg.DeclareLocal(typeof(long));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, ticks);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64)));

            // If the unpacker method failed (returned false), throw
            var lbl3 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl3);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int64 value for ticks component of DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl3);

            // Read offset
            var minutes = ilg.DeclareLocal(typeof(short));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, minutes);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt16)));

            // If the unpacker method failed (returned false), throw
            var lbl4 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl4);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int16 value for offset component of DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl4);

            // Compose the final DateTimeOffset
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Ldloc, ticks);
            ilg.Emit(OpCodes.Ldloc, minutes);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Ldc_I4, TicksPerMinute);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Mul);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Call, typeof(TimeSpan).GetMethod(nameof(TimeSpan.FromTicks), BindingFlags.Static | BindingFlags.Public));
            ilg.Emit(OpCodes.Call, typeof(DateTimeOffset).GetConstructor(new[] {typeof(long), typeof(TimeSpan)}));
        }
    }
}