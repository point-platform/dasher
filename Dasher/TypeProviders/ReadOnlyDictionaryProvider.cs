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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ReadOnlyDictionaryProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        public bool UseDefaultNullHandling(Type valueType) => true;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var dicType = value.LocalType;
            var readOnlyCollectionType = dicType.GetInterfaces().Single(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
            var enumerableType = dicType.GetInterfaces().Single(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
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
            ilg.Emit(OpCodes.Call, Methods.Packer_PackMapHeader);

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
            if (!SerialiserEmitter.TryEmitSerialiseCode(ilg, throwBlocks, errors, keyValue, packer, context, contextLocal))
            {
                errors.Add($"Cannot serialise IReadOnlyDictionary<> key type {keyType}.");
                return false;
            }

            // read value
            ilg.Emit(OpCodes.Ldloca, pairValue);
            ilg.Emit(OpCodes.Call, pairType.GetProperty(nameof(KeyValuePair<int,int>.Value)).GetMethod);
            ilg.Emit(OpCodes.Stloc, valueValue);

            // pack value
            if (!SerialiserEmitter.TryEmitSerialiseCode(ilg, throwBlocks, errors, valueValue, packer, context, contextLocal))
            {
                errors.Add($"Cannot serialise IReadOnlyDictionary<> value type {valueType}.");
                return false;
            }

            // progress enumerator & loop test
            ilg.MarkLabel(loopTest);
            ilg.Emit(OpCodes.Ldloc, enumerator);
            ilg.Emit(OpCodes.Callvirt, Methods.IEnumerator_MoveNext);
            ilg.Emit(OpCodes.Brtrue, loopStart);

            // finally
            ilg.BeginFinallyBlock();

            // dispose
            ilg.Emit(OpCodes.Ldloc, enumerator);
            ilg.Emit(OpCodes.Callvirt, Methods.IDisposable_Dispose);

            // end try/finally
            ilg.EndExceptionBlock();

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var rodicType = value.LocalType;
            var dicType = typeof(Dictionary<,>).MakeGenericType(rodicType.GenericTypeArguments);
            var keyType = rodicType.GetGenericArguments()[0];
            var valueType = rodicType.GetGenericArguments()[1];

            // read map length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadMapLength);

            // verify read correctly
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting collection data to be encoded a map");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            var keyValue = ilg.DeclareLocal(keyType);
            var valueValue = ilg.DeclareLocal(valueType);

            // create a mutable dictionary to store key/values
            var dic = ilg.DeclareLocal(dicType);
            ilg.Emit(OpCodes.Newobj, dicType.GetConstructor(Type.EmptyTypes));
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
            if (!DeserialiserEmitter.TryEmitDeserialiseCode(ilg, throwBlocks, errors, name, targetType, keyValue, unpacker, context, contextLocal, unexpectedFieldBehaviour))
            {
                errors.Add($"Unable to deserialise IReadOnlyDictionary<> key type {keyType} from MsgPack data.");
                return false;
            }

            // read value
            if (!DeserialiserEmitter.TryEmitDeserialiseCode(ilg, throwBlocks, errors, name, targetType, valueValue, unpacker, context, contextLocal, unexpectedFieldBehaviour))
            {
                errors.Add($"Unable to deserialise IReadOnlyDictionary<> value type {keyType} from MsgPack data.");
                return false;
            }

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

            return true;
        }
    }
}