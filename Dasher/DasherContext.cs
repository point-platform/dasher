using System;
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