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

namespace Dasher
{
    /// <summary>
    /// Enumeration of MsgPack value formats.
    /// </summary>
    public enum Format
    {
        #pragma warning disable 1591

        Unknown = 0,
        PositiveFixInt,
        FixMap,
        FixArray,
        FixStr,
        Null,
        False,
        True,
        Bin8,
        Bin16,
        Bin32,
        Ext8,
        Ext16,
        Ext32,
        Float32,
        Float64,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Int8,
        Int16,
        Int32,
        Int64,
        FixExt1,
        FixExt2,
        FixExt4,
        FixExt8,
        FixExt16,
        Str8,
        Str16,
        Str32,
        Array16,
        Array32,
        Map16,
        Map32,
        NegativeFixInt
    }
}