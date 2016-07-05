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
using Dasher.TypeProviders;

namespace Dasher
{
    public sealed class DasherContext
    {
        private static readonly ComplexTypeProvider _complexTypeProvider = new ComplexTypeProvider();
        private static readonly IReadOnlyList<ITypeProvider> _defaultTypeProviders = new ITypeProvider[]
        {
            new MsgPackTypeProvider(),
            new DecimalProvider(),
            new DateTimeProvider(),
            new DateTimeOffsetProvider(),
            new TimeSpanProvider(),
            new IntPtrProvider(),
            new GuidProvider(),
            new EnumProvider(),
            new VersionProvider(),
            new ReadOnlyListProvider(),
            new ReadOnlyDictionaryProvider(),
            new NullableValueProvider(),
            new TupleProvider(),
            new UnionProvider()
        };

        private readonly ConcurrentDictionary<Type, Action<UnsafePacker, DasherContext, object>> _serialiserByType = new ConcurrentDictionary<Type, Action<UnsafePacker, DasherContext, object>>();
        private readonly ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>> _deserialiserByType = new ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>>();
        private readonly IReadOnlyList<ITypeProvider> _typeProviders;

        public DasherContext(IEnumerable<ITypeProvider> typeProviders = null)
        {
            _typeProviders = typeProviders?.Concat(_defaultTypeProviders).ToList() ?? _defaultTypeProviders;
        }

        internal Action<UnsafePacker, DasherContext, object> GetOrCreateSerialiser(Type type)
            => _serialiserByType.GetOrAdd(type, _ => SerialiserEmitter.Build(type, this));

        internal Func<Unpacker, DasherContext, object> GetOrCreateDeserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
            => _deserialiserByType.GetOrAdd(
                Tuple.Create(type, unexpectedFieldBehaviour),
                _ => DeserialiserEmitter.Build(type, unexpectedFieldBehaviour, this));

        internal bool TryGetTypeProvider(Type type, out ITypeProvider provider)
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
    }
}