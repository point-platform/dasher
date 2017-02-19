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
using System.Reflection;
using Xunit;

namespace Dasher.Tests
{
    public class MethodsTests
    {
        [Fact]
        public void AllReflectedMethodsNonNull()
        {
            var properties = typeof(DasherContext).GetTypeInfo().Assembly.GetType("Dasher.Methods", throwOnError: true).GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(null);

                Assert.True(value != null, $"Dasher.Methods.{property.Name} shouldn't be null");
            }
        }
    }
}