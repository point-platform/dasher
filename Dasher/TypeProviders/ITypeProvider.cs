using System;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    // TODO nullable values

    public interface ITypeProvider
    {
        bool CanProvide(Type type);

        void Serialise(
            ILGenerator ilg,
            LocalBuilder value,
            LocalBuilder packer,
            DasherContext context);

        void Deserialise(
            ILGenerator ilg,
            string name,
            Type targetType,
            LocalBuilder value,
            LocalBuilder unpacker,
            LocalBuilder contextLocal,
            DasherContext context,
            UnexpectedFieldBehaviour unexpectedFieldBehaviour);
    }
}