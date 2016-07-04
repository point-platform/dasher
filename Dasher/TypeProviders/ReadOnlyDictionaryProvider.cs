using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ReadOnlyDictionaryProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var dicType = value.LocalType;
            var readOnlyCollectionType = dicType.GetInterfaces().Single(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
            var enumerableType = dicType.GetInterfaces().Single(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var pairType = enumerableType.GenericTypeArguments.Single();
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(pairType);
            var keyType = dicType.GetGenericArguments()[0];
            var valueType = dicType.GetGenericArguments()[1];

            // load packer (we'll write the size after loading it)
            ilg.Emit(OpCodes.Ldloc, packer);

            // read map size
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Callvirt, readOnlyCollectionType.GetProperty(nameof(IReadOnlyCollection<int>.Count)).GetMethod);

            // write map header
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackMapHeader)));

            var enumerator = ilg.DeclareLocal(enumeratorType);
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Callvirt, enumerableType.GetMethod(nameof(IEnumerable<int>.GetEnumerator)));
            ilg.Emit(OpCodes.Stloc, enumerator);

            var pairValue = ilg.DeclareLocal(pairType);
            var keyValue = ilg.DeclareLocal(keyType);
            var valueValue = ilg.DeclareLocal(valueType);

            // try
            ilg.BeginExceptionBlock();

            // begin loop
            var loopStart = ilg.DefineLabel();
            var loopTest = ilg.DefineLabel();

            ilg.Emit(OpCodes.Br, loopTest);
            ilg.MarkLabel(loopStart);

            // loop body
            ilg.Emit(OpCodes.Ldloc, enumerator);
            ilg.Emit(OpCodes.Callvirt, enumeratorType.GetProperty(nameof(IEnumerator<int>.Current)).GetMethod);
            ilg.Emit(OpCodes.Stloc, pairValue);

            // read key
            ilg.Emit(OpCodes.Ldloca, pairValue);
            ilg.Emit(OpCodes.Call, pairType.GetProperty(nameof(KeyValuePair<int,int>.Key)).GetMethod);
            ilg.Emit(OpCodes.Stloc, keyValue);

            // pack key
            if (!context.TrySerialise(ilg, keyValue, packer, contextLocal))
                throw new Exception($"Cannot serialise IReadOnlyDictionary<> key type {keyType}.");

            // read value
            ilg.Emit(OpCodes.Ldloca, pairValue);
            ilg.Emit(OpCodes.Call, pairType.GetProperty(nameof(KeyValuePair<int,int>.Value)).GetMethod);
            ilg.Emit(OpCodes.Stloc, valueValue);

            // pack value
            if (!context.TrySerialise(ilg, valueValue, packer, contextLocal))
                throw new Exception($"Cannot serialise IReadOnlyDictionary<> value type {valueValue}.");

            // progress enumerator & loop test
            ilg.MarkLabel(loopTest);
            ilg.Emit(OpCodes.Ldloc, enumerator);
            ilg.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));
            ilg.Emit(OpCodes.Brtrue, loopStart);

            // finally
            ilg.BeginFinallyBlock();

            // dispose
            ilg.Emit(OpCodes.Ldloc, enumerator);
            ilg.Emit(OpCodes.Callvirt, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));

            // end try/finally
            ilg.EndExceptionBlock();
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var rodicType = value.LocalType;
            var dicType = typeof(Dictionary<,>).MakeGenericType(rodicType.GenericTypeArguments);
            var keyType = rodicType.GetGenericArguments()[0];
            var valueType = rodicType.GetGenericArguments()[1];

            // read map length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadMapLength)));

            // verify read correctly
            var lbl1 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl1);
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting collection data to be encoded a map");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl1);

            var keyValue = ilg.DeclareLocal(keyType);
            var valueValue = ilg.DeclareLocal(valueType);

            // create a mutable dictionary to store key/values
            var dic = ilg.DeclareLocal(dicType);
            ilg.Emit(OpCodes.Newobj, dicType.GetConstructor(new Type[0]));
            ilg.Emit(OpCodes.Stloc, dic);

            // begin loop
            var loopStart = ilg.DefineLabel();
            var loopTest = ilg.DefineLabel();
            var loopEnd = ilg.DefineLabel();

            var i = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc, i);

            ilg.Emit(OpCodes.Br, loopTest);
            ilg.MarkLabel(loopStart);

            // loop body

            // read key
            if (!context.TryDeserialise(ilg, name, targetType, keyValue, unpacker, contextLocal, unexpectedFieldBehaviour))
                throw new Exception($"Unable to deserialise values of type {keyType} from MsgPack data.");

            // read value
            if (!context.TryDeserialise(ilg, name, targetType, valueValue, unpacker, contextLocal, unexpectedFieldBehaviour))
                throw new Exception($"Unable to deserialise values of type {valueValue} from MsgPack data.");

            ilg.Emit(OpCodes.Ldloc, dic);
            ilg.Emit(OpCodes.Ldloc, keyValue);
            ilg.Emit(OpCodes.Ldloc, valueValue);
            ilg.Emit(OpCodes.Callvirt, dicType.GetMethod(nameof(Dictionary<int, int>.Add)));

            // loop counter increment
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc, i);

            // loop test
            ilg.MarkLabel(loopTest);
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Clt);
            ilg.Emit(OpCodes.Brtrue, loopStart);

            // after loop
            ilg.MarkLabel(loopEnd);

            ilg.Emit(OpCodes.Ldloc, dic);
            ilg.Emit(OpCodes.Stloc, value);
        }
    }
}