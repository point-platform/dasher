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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Dasher.Utils
{
    internal sealed class NumericStringComparer : IComparer<string>
    {
        public static NumericStringComparer Default { get; } = new NumericStringComparer();

        public int Compare([CanBeNull] string x, [CanBeNull] string y)
        {
            // sort nulls to the start
            if (x == null)
                return y == null ? 0 : -1;
            if (y == null)
                return 1;

            var ix = 0;
            var iy = 0;

            while (true)
            {
                // sort shorter strings to the start
                if (ix >= x.Length)
                    return iy >= y.Length ? 0 : -1;
                if (iy >= y.Length)
                    return 1;

                var cx = x[ix];
                var cy = y[iy];

                int result;
                if (char.IsDigit(cx) && char.IsDigit(cy))
                    result = CompareInteger(x, y, ref ix, ref iy);
                else
                    result = cx.CompareTo(y[iy]);

                if (result != 0)
                    return result;

                ix++;
                iy++;
            }
        }

        private static int CompareInteger(string x, string y, ref int ix, ref int iy)
        {
            var lx = GetNumLength(x, ix);
            var ly = GetNumLength(y, iy);

            // shorter number first (note, doesn't handle leading zeroes)
            if (lx != ly)
                return lx.CompareTo(ly);

            for (var i = 0; i < lx; i++)
            {
                var result = x[ix++].CompareTo(y[iy++]);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        private static int GetNumLength(string s, int i)
        {
            var length = 0;
            while (i < s.Length && char.IsDigit(s[i++]))
                length++;
            return length;
        }
    }
}