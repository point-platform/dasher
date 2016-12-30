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
using System.Collections;
using System.Collections.Generic;
#if NETSTANDARD1_3
using System.Reflection;
#endif

namespace Dasher.Utils
{
    internal static class EnumerableExtensions
    {
        public static bool SequenceEqual<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> comparer)
        {
            if (typeof(ICollection).IsAssignableFrom(typeof(T)))
            {
                if (((ICollection)a).Count != ((ICollection)b).Count)
                    return false;
            }

            using (var ae = a.GetEnumerator())
            using (var be = b.GetEnumerator())
            {
                while (ae.MoveNext())
                {
                    if (!be.MoveNext())
                        return false;

                    if (!comparer(ae.Current, be.Current))
                        return false;
                }

                if (be.MoveNext())
                    return false;

                return true;
            }
        }
    }
}