#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dasher.TypeProviders;
using Dasher.Utils;

namespace Dasher
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Methods
    {
        public static ConstructorInfo Exception_Ctor_String                     { get; } = typeof(Exception).GetConstructor(new[] {typeof(string)});

        public static ConstructorInfo DeserialisationException_Ctor_String_Type { get; } = typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)});

        public static MethodInfo Object_ToString                                { get; } = typeof(object).GetMethod(nameof(ToString), Type.EmptyTypes);

        public static MethodInfo String_Equals_String_String                    { get; } = typeof(string).GetMethod(nameof(string.Equals), new[] {typeof(string), typeof(string)});
        public static MethodInfo String_Equals_String_StringComparison          { get; } = typeof(string).GetMethod(nameof(string.Equals), new[] {typeof(string), typeof(StringComparison)});
        public static MethodInfo String_Format_String_Object                    { get; } = typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object)});
        public static MethodInfo String_Format_String_Object_Object             { get; } = typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object), typeof(object)});
        public static MethodInfo String_Format_String_Object_Object_Object      { get; } = typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object), typeof(object), typeof(object)});
        public static MethodInfo String_GetLength                               { get; } = typeof(string).GetProperty(nameof(string.Length)).GetMethod;
        public static MethodInfo String_Indexer                                 { get; } = typeof(string).GetProperties(BindingFlags.Instance | BindingFlags.Public).Single(p => p.GetIndexParameters().Length == 1 && p.PropertyType == typeof(char)).GetMethod;

        public static MethodInfo Unpacker_TryReadString                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] {typeof(string).MakeByRefType()});
        public static MethodInfo Unpacker_TryReadArrayLength                    { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength));
        public static MethodInfo Unpacker_TryReadMapLength                      { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadMapLength));
        public static MethodInfo Unpacker_TryReadNull                           { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull));
        public static MethodInfo Unpacker_TryPeekFormat                         { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryPeekFormat));
        public static MethodInfo Unpacker_TryPeekEmptyMap                       { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryPeekEmptyMap));
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
        public static MethodInfo Unpacker_TryReadByteArraySegment               { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadByteArraySegment), new[] {typeof(ArraySegment<byte>).MakeByRefType()});
        public static MethodInfo Unpacker_SkipValue                             { get; } = typeof(Unpacker).GetMethod(nameof(Unpacker.SkipValue));
        public static MethodInfo Unpacker_HasStreamEnded_Get                    { get; } = typeof(Unpacker).GetProperty(nameof(Unpacker.HasStreamEnded)).GetMethod;

        public static MethodInfo Packer_PackArrayHeader                         { get; } = typeof(Packer).GetMethod(nameof(Packer.PackArrayHeader));
        public static MethodInfo Packer_PackMapHeader                           { get; } = typeof(Packer).GetMethod(nameof(Packer.PackMapHeader));
        public static MethodInfo Packer_PackNull                                { get; } = typeof(Packer).GetMethod(nameof(Packer.PackNull));
        public static MethodInfo Packer_Pack_String                             { get; } = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {typeof(string)});
        public static MethodInfo Packer_Pack_Int16                              { get; } = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {typeof(short)});
        public static MethodInfo Packer_Pack_Int64                              { get; } = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {typeof(long)});
        public static MethodInfo Packer_Pack_ByteArray                          { get; } = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {typeof(byte[])});
        public static MethodInfo Packer_Pack_ByteArraySegment                   { get; } = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {typeof(ArraySegment<byte>)});

        public static MethodInfo Object_Equals_Object_Object                    { get; } = typeof(object).GetMethod(nameof(object.Equals), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo UnionEncoding_GetTypeName                      { get; } = typeof(UnionEncoding).GetMethod(nameof(UnionEncoding.GetTypeName), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo DasherDeserialiseFunc_Invoke                   { get; } = typeof(Func<Unpacker, DasherContext, object>).GetMethod(nameof(Func<Unpacker, DasherContext, object>.Invoke), new[] {typeof(Unpacker), typeof(DasherContext)});
        public static MethodInfo DasherSerialiseAction_Invoke                   { get; } = typeof(Action<Packer, DasherContext, object>).GetMethod(nameof(Func<Packer, DasherContext, object>.Invoke), new[] {typeof(Packer), typeof(DasherContext), typeof(object)});

        public static MethodInfo DasherContext_GetOrCreateDeserialiseFunc       { get; } = typeof(DasherContext).GetMethod(nameof(DasherContext.GetOrCreateDeserialiseFunc), BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo DasherContext_GetOrCreateSerialiseAction       { get; } = typeof(DasherContext).GetMethod(nameof(DasherContext.GetOrCreateSerialiseAction), BindingFlags.Instance | BindingFlags.NonPublic);

        public static MethodInfo Format_ToString                                { get; } = typeof(Format).GetMethod(nameof(Format.ToString), Type.EmptyTypes);

        public static MethodInfo DateTime_ToBinary                              { get; } = typeof(DateTime).GetMethod(nameof(DateTime.ToBinary));
        public static MethodInfo DateTime_FromBinary                            { get; } = typeof(DateTime).GetMethod(nameof(DateTime.FromBinary), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo DateTimeOffset_Ticks_Get                       { get; } = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Ticks)).GetMethod;
        public static MethodInfo DateTimeOffset_Offset_Get                      { get; } = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Offset)).GetMethod;
        public static ConstructorInfo DateTimeOffset_Ctor_Long_TimeSpan         { get; } = typeof(DateTimeOffset).GetConstructor(new[] {typeof(long), typeof(TimeSpan)});

        public static MethodInfo TimeSpan_Ticks_Get                             { get; } = typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)).GetMethod;
        public static MethodInfo TimeSpan_FromTicks                             { get; } = typeof(TimeSpan).GetMethod(nameof(TimeSpan.FromTicks), BindingFlags.Static | BindingFlags.Public);
        public static ConstructorInfo TimeSpan_Ctor_Int64                       { get; } = typeof(TimeSpan).GetConstructor(new[] {typeof(long)});

        public static MethodInfo Decimal_ToString                               { get; } = typeof(decimal).GetMethod(nameof(decimal.ToString), Type.EmptyTypes);
        public static MethodInfo Decimal_TryParse                               { get; } = typeof(decimal).GetMethod(nameof(decimal.TryParse), new[] {typeof(string), typeof(decimal).MakeByRefType()});
        public static ConstructorInfo Decimal_Ctor_IntArray                     { get; } = typeof(decimal).GetConstructor(new[] {typeof(int[])});

        public static ConstructorInfo ArraySegment_Ctor_ByteArray               { get; } = typeof(ArraySegment<byte>).GetConstructor(new[] {typeof(byte[])});

        public static MethodInfo Char_ToString                                  { get; } = typeof(char).GetMethod(nameof(char.ToString), Type.EmptyTypes);

        public static MethodInfo Enum_TryParse_OpenGeneric                      { get; } = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == nameof(Enum.TryParse) && m.GetParameters().Length == 3);

        public static MethodInfo Guid_ToByteArray                               { get; } = typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Type.EmptyTypes);
        public static ConstructorInfo Guid_Constructor_ByteArray                { get; } = typeof(Guid).GetConstructor(new[] {typeof(byte[])});

        public static MethodInfo IntPtr_ToInt64                                 { get; } = typeof(IntPtr).GetMethod(nameof(IntPtr.ToInt64));
        public static ConstructorInfo IntPtr_Ctor_Int64                         { get; } = typeof(IntPtr).GetConstructor(new[] {typeof(long)});

        public static MethodInfo IEnumerator_MoveNext                           { get; } = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));

        public static MethodInfo IDisposable_Dispose                            { get; } = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));

        public static MethodInfo Version_ToString                               { get; } = typeof(Version).GetMethod(nameof(Version.ToString), Type.EmptyTypes);

        public static MethodInfo DecimalProvider_Parse                          { get; } = typeof(DecimalProvider).GetMethod(nameof(DecimalProvider.Parse), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo EmptyArrayInstanceGetter(Type elementType)     => typeof(EmptyArray<>).MakeGenericType(elementType).GetProperty(nameof(EmptyArray<byte>.Instance)).GetMethod;
    }
}
