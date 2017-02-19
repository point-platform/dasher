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
using System.Collections.Generic;

namespace Dasher.Tests
{
    internal sealed class SelectiveStringComparer : IEqualityComparer<string>
    {
        private readonly string _ignoreChars;

        public SelectiveStringComparer(string ignoreChars = "\r\n")
        {
            _ignoreChars = ignoreChars;
        }

        public bool Equals(string x, string y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            var ix = 0;
            var iy = 0;
            while (true)
            {
                while (ix < x.Length && _ignoreChars.IndexOf(x[ix]) != -1)
                    ix++;
                while (iy < y.Length && _ignoreChars.IndexOf(y[iy]) != -1)
                    iy++;
                if (ix >= x.Length)
                    return iy >= y.Length;
                if (iy >= y.Length)
                    return false;
                if (x[ix] != y[iy])
                    return false;
                ix++;
                iy++;
            }
        }

        public int GetHashCode(string obj)
        {
            throw new NotSupportedException();
        }
    }
}