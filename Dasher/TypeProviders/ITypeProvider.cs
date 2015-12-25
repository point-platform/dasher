using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    public interface ITypeProvider
    {
        bool CanProvide(Type type);

        void Serialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer);

        void Deserialise(ILGenerator ilg, LocalBuilder value, LocalBuilder unpacker, string name, Type targetType);
    }

    internal static class TypeProviders
    {
        public static IEnumerable<ITypeProvider> Default { get; } = new ITypeProvider[]
        {
            new MsgPackTypeProvider(),
            new DecimalProvider(),
            new EnumProvider()
        };
    }
}