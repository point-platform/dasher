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

namespace Dasher.Tests
{
    internal static class TestUtils
    {
        public static void CleanUpForPerfTest()
        {
            GC.Collect(2, GCCollectionMode.Forced);

#if !NETCOREAPP1_0
            GC.WaitForFullGCComplete(1000);
#endif

            GC.WaitForPendingFinalizers();
        }
    }
}