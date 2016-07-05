using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dasher.TypeProviders;

namespace Dasher
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Methods
    {
        public static ConstructorInfo Exception_Ctor_String                     { get; } = typeof(Exception).GetConstructor(new[] {typeof(string)});

        public static ConstructorInfo DeserialisationException_Ctor_String_Type { get; } = typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)});

        public static MethodInfo Object_ToString                                { get; } = typeof(object).GetMethod(nameof(ToString), new Type[0]);

        public static MethodInfo String_Equals_String_String                    { get; } = typeof(string).GetMethod(nameof(string.Equals), BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(string), typeof(string)}, null);
        public static MethodInfo String_Equals_String_StringComparison          { get; } = typeof(string).GetMethod(nameof(string.Equals), new[] {typeof(string), typeof(StringComparison)});
        public static MethodInfo String_Format_String_Object_Object             { get; } = typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object), typeof(object)});
        public static MethodInfo String_Format_String_Object_Object_Object      { get; } = typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object), typeof(object), typeof(object)});

        public static MethodInfo Unpacker_TryReadString                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] {typeof(string).MakeByRefType()});
        public static MethodInfo Unpacker_TryReadArrayLength                    { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength));
        public static MethodInfo Unpacker_TryReadMapLength                      { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadMapLength));
        public static MethodInfo Unpacker_TryReadNull                           { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull));
        public static MethodInfo Unpacker_TryPeekFormat                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryPeekFormat));
        public static MethodInfo Unpacker_TryReadSByte                          { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSByte));
        public static MethodInfo Unpacker_TryReadByte                           { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadByte));
        public static MethodInfo Unpacker_TryReadInt16                          { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt16));
        public static MethodInfo Unpacker_TryReadUInt16                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt16));
        public static MethodInfo Unpacker_TryReadInt32                          { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt32));
        public static MethodInfo Unpacker_TryReadUInt32                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt32));
        public static MethodInfo Unpacker_TryReadInt64                          { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64));
        public static MethodInfo Unpacker_TryReadUInt64                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt64));
        public static MethodInfo Unpacker_TryReadSingle                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSingle));
        public static MethodInfo Unpacker_TryReadDouble                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadDouble));
        public static MethodInfo Unpacker_TryReadBoolean                        { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBoolean));
        public static MethodInfo Unpacker_TryReadBinary                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBinary), new[] {typeof(byte[]).MakeByRefType()});
        public static MethodInfo Unpacker_SkipValue                             { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.SkipValue));
        public static MethodInfo Unpacker_HasStreamEnded_Get                    { get; } = typeof(Unpacker).GetProperty(nameof(Unpacker.HasStreamEnded)).GetMethod;

        public static MethodInfo UnsafePacker_PackArrayHeader                   { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackArrayHeader));
        public static MethodInfo UnsafePacker_PackMapHeader                     { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackMapHeader));
        public static MethodInfo UnsafePacker_PackNull                          { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackNull));
        public static MethodInfo UnsafePacker_Pack_String                       { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(string)});
        public static MethodInfo UnsafePacker_Pack_Int16                        { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(short)});
        public static MethodInfo UnsafePacker_Pack_Int64                        { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(long)});
        public static MethodInfo UnsafePacker_Pack_ByteArray                    { get; } = typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(byte[])});

        public static MethodInfo Object_Equals_Object_Object                    { get; } = typeof(object).GetMethod(nameof(object.Equals), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo UnionProvider_GetTypeName                      { get; } = typeof(UnionProvider).GetMethod(nameof(UnionProvider.GetTypeName), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo DasherDeserialiseFunc_Invoke                   { get; } = typeof(Func<Unpacker, DasherContext, object>).GetMethod(nameof(Func<Unpacker, DasherContext, object>.Invoke), new[] {typeof(Unpacker), typeof(DasherContext)});
        public static MethodInfo DasherSerialiseAction_Invoke                   { get; } = typeof(Action<UnsafePacker, DasherContext, object>).GetMethod(nameof(Func<UnsafePacker, DasherContext, object>.Invoke), new[] {typeof(UnsafePacker), typeof(DasherContext), typeof(object)});

        public static MethodInfo DasherContext_GetOrCreateDeserialiseFunc       { get; } = typeof(DasherContext).GetMethod(nameof(DasherContext.GetOrCreateDeserialiseFunc), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(Type), typeof(UnexpectedFieldBehaviour)}, null);
        public static MethodInfo DasherContext_GetOrCreateSerialiseAction       { get; } = typeof(DasherContext).GetMethod(nameof(DasherContext.GetOrCreateSerialiseAction), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(Type)}, null);

        public static MethodInfo Format_ToString                                { get; } = typeof(Format).GetMethod(nameof(Format.ToString), new Type[0]);

        public static MethodInfo DateTime_ToBinary                              { get; } = typeof(DateTime).GetMethod(nameof(DateTime.ToBinary));
        public static MethodInfo DateTime_FromBinary                            { get; } = typeof(DateTime).GetMethod(nameof(DateTime.FromBinary), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo DateTimeOffset_Ticks_Get                       { get; } = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Ticks)).GetMethod;
        public static MethodInfo DateTimeOffset_Offset_Get                      { get; } = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Offset)).GetMethod;
        public static ConstructorInfo DateTimeOffset_Ctor_Long_TimeSpan         { get; } = typeof(DateTimeOffset).GetConstructor(new[] {typeof(long), typeof(TimeSpan)});

        public static MethodInfo TimeSpan_Ticks_Get                             { get; } = typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)).GetMethod;
        public static MethodInfo TimeSpan_FromTicks                             { get; } = typeof(TimeSpan).GetMethod(nameof(TimeSpan.FromTicks), BindingFlags.Static | BindingFlags.Public);
        public static ConstructorInfo TimeSpan_Ctor_Int64                       { get; } = typeof(TimeSpan).GetConstructor(new[] {typeof(long)});

        public static MethodInfo Decimal_ToString                               { get; } = typeof(decimal).GetMethod(nameof(decimal.ToString), new Type[0]);
        public static MethodInfo Decimal_TryParse                               { get; } = typeof(decimal).GetMethod(nameof(decimal.TryParse), new[] {typeof(string), typeof(decimal).MakeByRefType()});
        public static ConstructorInfo Decimal_Ctor_IntArray                     { get; } = typeof(decimal).GetConstructor(new[] {typeof(int[])});

        public static MethodInfo Enum_TryParse_OpenGeneric                      { get; } = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == nameof(Enum.TryParse) && m.GetParameters().Length == 3);

        public static MethodInfo Guid_ToByteArray                               { get; } = typeof(Guid).GetMethod(nameof(Guid.ToByteArray), new Type[0]);
        public static ConstructorInfo Guid_Constructor_ByteArray                { get; } = typeof(Guid).GetConstructor(new[] {typeof(byte[])});

        public static MethodInfo IntPtr_ToInt64                                 { get; } = typeof(IntPtr).GetMethod(nameof(IntPtr.ToInt64));
        public static ConstructorInfo IntPtr_Ctor_Int64                         { get; } = typeof(IntPtr).GetConstructor(new[] {typeof(long)});

        public static MethodInfo IEnumerator_MoveNext                           { get; } = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));

        public static MethodInfo IDisposable_Dispose                            { get; } = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));

        public static MethodInfo Version_ToString                               { get; } = typeof(Version).GetMethod(nameof(Version.ToString), new Type[0]);
    }
}