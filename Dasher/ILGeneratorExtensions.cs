using System;
using System.Reflection.Emit;

namespace Dasher
{
    internal static class ILGeneratorExtensions
    {
        public static void LoadType(this ILGenerator ilg, Type type)
        {
            ilg.Emit(OpCodes.Ldtoken, type);
            ilg.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
        }
    }
}