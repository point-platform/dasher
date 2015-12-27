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

namespace Dasher
{
    internal static class MsgPackConstants
    {
        public const byte PosFixIntMaxValue       = 0x7F;
        public const byte PosFixIntPrefixBits     = 0x00;
        public const byte PosFixIntPrefixBitsMask = 0x80;
        public const byte PosFixIntMaxByte        = PosFixIntPrefixBits | PosFixIntMaxValue;

        public const sbyte NegFixIntMinValue      = -32; // 0b1110000
        public const byte NegFixIntPrefixBits     = 0xE0;
        public const byte NegFixIntPrefixBitsMask = 0xE0;
        public const byte NegFixIntMinByte        = 0xE0;

        public const byte FixMapMaxLength         = 0x0F;
        public const byte FixMapPrefixBits        = 0x80;
        public const byte FixMapPrefixBitsMask    = 0xF0;
        public const byte FixMapMinPrefixByte     = FixMapPrefixBits;
        public const byte FixMapMaxPrefixByte     = FixMapPrefixBits | FixMapMaxLength;

        public const byte FixArrayMaxLength       = 0x0F;
        public const byte FixArrayPrefixBits      = 0x90;
        public const byte FixArrayPrefixBitsMask  = 0xF0;
        public const byte FixArrayMinPrefixByte   = FixArrayPrefixBits;
        public const byte FixArrayMaxPrefixByte   = FixArrayPrefixBits | FixArrayMaxLength;

        public const byte FixStrMaxLength         = 0x1F;
        public const byte FixStrPrefixBits        = 0xA0;
        public const byte FixStrPrefixBitsMask    = 0xE0;
        public const byte FixStrMinPrefixByte     = FixStrPrefixBits;
        public const byte FixStrMaxPrefixByte     = FixStrPrefixBits | FixStrMaxLength;

        public const byte NullByte                = 0xC0;
        public const byte FalseByte               = 0xC2;
        public const byte TrueByte                = 0xC3;

        public const byte Bin8PrefixByte          = 0xC4;
        public const byte Bin16PrefixByte         = 0xC5;
        public const byte Bin32PrefixByte         = 0xC6;
        public const byte Ext8PrefixByte          = 0xC7;
        public const byte Ext16PrefixByte         = 0xC8;
        public const byte Ext32PrefixByte         = 0xC9;
        public const byte Float32PrefixByte       = 0xCA;
        public const byte Float64PrefixByte       = 0xCB;
        public const byte UInt8PrefixByte         = 0xCC;
        public const byte UInt16PrefixByte        = 0xCD;
        public const byte UInt32PrefixByte        = 0xCE;
        public const byte UInt64PrefixByte        = 0xCF;
        public const byte Int8PrefixByte          = 0xD0;
        public const byte Int16PrefixByte         = 0xD1;
        public const byte Int32PrefixByte         = 0xD2;
        public const byte Int64PrefixByte         = 0xD3;
        public const byte FixExt1PrefixByte       = 0xD4;
        public const byte FixExt2PrefixByte       = 0xD5;
        public const byte FixExt4PrefixByte       = 0xD6;
        public const byte FixExt8PrefixByte       = 0xD7;
        public const byte FixExt16PrefixByte      = 0xD8;
        public const byte Str8PrefixByte          = 0xD9;
        public const byte Str16PrefixByte         = 0xDA;
        public const byte Str32PrefixByte         = 0xDB;
        public const byte Array16PrefixByte       = 0xDC;
        public const byte Array32PrefixByte       = 0xDD;
        public const byte Map16PrefixByte         = 0xDE;
        public const byte Map32PrefixByte         = 0xDF;
    }
}