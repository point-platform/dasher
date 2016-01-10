#region License
//
// Dasher
//
// Copyright 2015 Drew Noakes
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

        private readonly ConcurrentDictionary<Type, Serialiser> _serialiserByType = new ConcurrentDictionary<Type, Serialiser>();

        internal void RegisterSerialiser(Type type, Serialiser serialiser)
        {
            _serialiserByType.TryAdd(type, serialiser);
        }

        internal Serialiser GetOrCreateSerialiser(Type type)
        {
            return _serialiserByType.GetOrAdd(type, t => new Serialiser(t, this));
        }

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