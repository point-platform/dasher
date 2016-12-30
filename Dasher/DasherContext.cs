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
    /// <summary>
    /// Contains metadata specific to a set of Dasher operations.
    /// </summary>
    /// <remarks>
    /// The context models the set of <see cref="ITypeProvider"/> instances used during serialisation
    /// and deserialisation.
    /// <para />
    /// It also caches code generated during the creation of <see cref="Serialiser"/> and <see cref="Deserialiser"/>
    /// (and their generic counterparts), allowing reuse which reduces memory consumption and time waiting for
    /// code generation/JIT compilation to complete.
    /// </remarks>
    public sealed class DasherContext
    {
        private static readonly ComplexTypeProvider _complexTypeProvider = new ComplexTypeProvider();
        private static readonly IReadOnlyList<ITypeProvider> _defaultTypeProviders = new ITypeProvider[]
        {
            new MsgPackTypeProvider(),
            new CharProvider(),
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
            new UnionProvider(),
            new EmptyProvider()
        };

        private readonly ConcurrentDictionary<Type, Action<Packer, DasherContext, object>> _serialiseActionByType = new ConcurrentDictionary<Type, Action<Packer, DasherContext, object>>();
        private readonly ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>> _deserialiseFuncByType = new ConcurrentDictionary<Tuple<Type, UnexpectedFieldBehaviour>, Func<Unpacker, DasherContext, object>>();
        private readonly IReadOnlyList<ITypeProvider> _typeProviders;

        /// <summary>
        /// Initialises a new Dasher context.
        /// </summary>
        /// <remarks>
        /// The caller may optionally provide one or more <see cref="ITypeProvider"/> instances to
        /// be included in the context. Any providers are prepended to the default set, allowing
        /// them to override built-in behaviour for one or more types.
        /// </remarks>
        /// <param name="typeProviders">Optional set of type providers, or <c>null</c>.</param>
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
                    errors.Add($"Multiple type providers exist for type \"{type}\".");
                    provider = null;
                    return false;
                }

                found = p;
            }

            if (found == null && _complexTypeProvider.CanProvide(type))
                found = _complexTypeProvider;

            provider = found;

            if (found == null)
                errors.Add($"No type provider exists for type \"{type}\".");

            return found != null;
        }

        internal void ValidateTopLevelType(Type type, ICollection<string> errors)
        {
            if (type == typeof(Empty))
                return;

            var nullableInnerType = Nullable.GetUnderlyingType(type);
            if (nullableInnerType != null)
            {
                ValidateTopLevelType(nullableInnerType, errors);
                return;
            }

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

        /// <summary>
        /// Get whether <paramref name="type"/> is valid as a top-level type for serialisation and deserialisation.
        /// </summary>
        /// <remarks>
        /// Dasher imposes some restrictions upon top-level types to allow contracts to evolve over time.
        /// Top-level types must be complex (classes, structs or nullable structs), or a <c>Union</c> type.
        /// </remarks>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is a valid top-level type, otherwise <c>false</c>.</returns>
        public bool IsValidTopLevelType(Type type)
        {
            var errors = new List<string>(capacity: 0);
            ValidateTopLevelType(type, errors);
            return errors.Count == 0;
        }
    }
}