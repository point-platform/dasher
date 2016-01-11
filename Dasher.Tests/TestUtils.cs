using System;

namespace Dasher.Tests
{
    internal static class TestUtils
    {
        internal static void CleanUpForPerfTest()
        {
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
            GC.WaitForPendingFinalizers();
        }
    }
}