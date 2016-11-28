using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Dasher.Schemata.Utils
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