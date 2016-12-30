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

namespace Dasher.Contracts
{
    public static class ContractExtensions
    {
        public static string ToReferenceString(this IWriteContract contract) => ToReferenceStringInternal(contract);

        public static string ToReferenceString(this IReadContract contract) => ToReferenceStringInternal(contract);

        private static string ToReferenceStringInternal(object contract)
        {
            var byRefContract = contract as ByRefContract;

            if (byRefContract != null)
            {
                if (string.IsNullOrWhiteSpace(byRefContract.Id))
                    throw new Exception("ByRefContract must have an ID to produce a reference string.");
                return '#' + byRefContract.Id;
            }

            return ((ByValueContract)contract).MarkupValue;
        }
    }
}
