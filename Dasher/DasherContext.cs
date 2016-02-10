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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dasher.TypeProviders;

namespace Dasher
{
    public sealed class DasherContext
    {
        private static readonly ComplexTypeProvider _complexTypeProvider = new ComplexTypeProvider();

        private readonly ConcurrentDictionary<Type, Action<UnsafePacker, DasherContext, object>> _serialiserByType = new ConcurrentDictionary<Type, Action<UnsafePacker, DasherContext, object>>();
        private readonly ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>> _deserialiserByType = new ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>>();
        private readonly IReadOnlyList<ITypeProvider> _typeProviders;

        public DasherContext(IEnumerable<ITypeProvider> typeProviders = null)
        {
            var defaults = new ITypeProvider[]
            {
                new MsgPackTypeProvider(),
                new DecimalProvider(),
                new DateTimeProvider(),
                new TimeSpanProvider(),
                new IntPtrProvider(),
                new GuidProvider(),
                new EnumProvider(),
                new VersionProvider(),
                new ReadOnlyListProvider(),
                new NullableValueProvider()
            };

            if (typeProviders == null)
                _typeProviders = defaults;
            else
                _typeProviders = typeProviders.Concat(defaults).ToList();
        }

        internal Action<UnsafePacker, DasherContext, object> GetOrCreateSerialiser(Type type)
            => _serialiserByType.GetOrAdd(type, _ => SerialiserEmitter.Build(type, this));

        internal Func<Unpacker, DasherContext, object> GetOrCreateDeserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
            => _deserialiserByType.GetOrAdd(
                Tuple.Create(type, unexpectedFieldBehaviour),
                _ => DeserialiserEmitter.Build(type, unexpectedFieldBehaviour, this));

        private bool TryGetTypeProvider(Type type, out ITypeProvider provider)
        {
            ITypeProvider found = null;
            foreach (var p in _typeProviders)
            {
                if (!p.CanProvide(type))
                    continue;
                if (found != null)
                    throw new Exception($"Multiple type providers exist for type {type}.");
                found = p;
            }

            if (found == null && _complexTypeProvider.CanProvide(type))
                found = _complexTypeProvider;

            provider = found;
            return found != null;
        }

        internal bool TrySerialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, bool isRoot = false)
        {
            ITypeProvider provider;
            if (!TryGetTypeProvider(value.LocalType, out provider))
                return false;

            if (!isRoot && provider is ComplexTypeProvider)
            {
                // prevent endless code generation for recursive types by delegating to a method call
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Call, typeof(DasherContext).GetMethod(nameof(GetOrCreateSerialiser), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(Type)}, null));

                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.Emit(OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Call, typeof(Action<UnsafePacker, DasherContext, object>).GetMethod(nameof(Func<UnsafePacker, DasherContext, object>.Invoke), new[] {typeof(UnsafePacker), typeof(DasherContext), typeof(object)}));
            }
            else
            {
                var end = ilg.DefineLabel();

                if (!value.LocalType.IsValueType)
                {
                    var nonNull = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Ldloc, value);
                    ilg.Emit(OpCodes.Brtrue, nonNull);
                    ilg.Emit(OpCodes.Ldloc, packer);
                    ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackNull)));
                    ilg.Emit(OpCodes.Br, end);
                    ilg.MarkLabel(nonNull);
                }

                provider.Serialise(ilg, value, packer, contextLocal, this);

                ilg.MarkLabel(end);
            }
            return true;
        }

        internal bool TryDeserialise(ILGenerator ilg, string name, Type targetType, LocalBuilder valueLocal, LocalBuilder unpacker, LocalBuilder contextLocal, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            ITypeProvider provider;
            if (!TryGetTypeProvider(valueLocal.LocalType, out provider))
                return false;

            var end = ilg.DefineLabel();

            if (!valueLocal.LocalType.IsValueType)
            {
                // check for null
                var nonNullLabel = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldloc, unpacker);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull)));
                ilg.Emit(OpCodes.Brfalse, nonNullLabel);
                {
                    ilg.Emit(OpCodes.Ldnull);
                    ilg.Emit(OpCodes.Stloc, valueLocal);
                    ilg.Emit(OpCodes.Br, end);
                }
                ilg.MarkLabel(nonNullLabel);
            }

            provider.Deserialise(ilg, name, targetType, valueLocal, unpacker, contextLocal, this, unexpectedFieldBehaviour);

            ilg.MarkLabel(end);
            return true;
        }
    }
}