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

        private readonly ConcurrentDictionary<Type, Action<Packer, DasherContext, object>> _serialiseActionByType = new ConcurrentDictionary<Type, Action<Packer, DasherContext, object>>();
        private readonly ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>> _deserialiseFuncByType = new ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>>();
        private readonly IReadOnlyList<ITypeProvider> _typeProviders;

        public DasherContext(IEnumerable<ITypeProvider> typeProviders = null)
        {
            _typeProviders = typeProviders?.Concat(_defaultTypeProviders).ToList() ?? _defaultTypeProviders;
        }

        internal Action<Packer, DasherContext, object> GetOrCreateSerialiseAction(Type type)
            => _serialiseActionByType.GetOrAdd(type, _ => SerialiserEmitter.Build(type, this));

        internal Func<Unpacker, DasherContext, object> GetOrCreateDeserialiseFunc(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
            => _deserialiseFuncByType.GetOrAdd(
                Tuple.Create(type, unexpectedFieldBehaviour),
                _ => DeserialiserEmitter.Build(type, unexpectedFieldBehaviour, this));

        internal bool TryGetTypeProvider(Type type, ICollection<string> errors, out ITypeProvider provider)
        {
            ITypeProvider found = null;
            foreach (var p in _typeProviders)
            {
                if (!p.CanProvide(type))
                    continue;

                // Disallow providers to overlap in their capabilities.
                // This property makes them run indeptendent of execution order.
                // If we wish to allow overlap (to override behaviour, for example) then we need
                // to allow control over the order of registered type providers.
                if (found != null)
                {
                    errors.Add($"Multiple type providers exist for type {type}.");
                    provider = null;
                    return false;
                }

                found = p;
            }

            if (found == null && _complexTypeProvider.CanProvide(type))
                found = _complexTypeProvider;

            provider = found;
            return found != null;
        }

        internal void ValidateTopLevelType(Type type, ICollection<string> errors)
        {
            if (Union.IsUnionType(type))
            {
                foreach (var memberType in Union.GetTypes(type))
                    ValidateTopLevelType(memberType, errors);
            }
            else if (_typeProviders.Any(p => p.CanProvide(type)))
            {
                errors.Add("Top level types must be complex to support future versioning.");
            }
            else
            {
                ComplexTypeProvider.TryValidateComplexType(type, errors);
            }
        }

        public bool IsValidTopLevelType(Type type)
        {
            var errors = new List<string>(capacity: 0);
            ValidateTopLevelType(type, errors);
            return errors.Count == 0;
        }
    }
}