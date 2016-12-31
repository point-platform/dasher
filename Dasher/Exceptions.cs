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
using System.Collections.Generic;
using System.Text;

namespace Dasher
{
    /// <summary>
    /// Raised for errors during the generation of a deserialiser.
    /// </summary>
    public sealed class DeserialiserGenerationException : Exception
    {
        /// <summary>
        /// The type to which the deserialiser generation exception relates.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initialises a new deserialiser generation exception.
        /// </summary>
        /// <param name="errors">Messages explaining the causes of the exception.</param>
        /// <param name="targetType">The type to which the deserialiser generation exception relates.</param>
        public DeserialiserGenerationException(IReadOnlyList<string> errors, Type targetType)
            : base(DasherExceptionUtil.CreateMessageFromErrors("Cannot generate deserialiser for type", errors, targetType))
        {
            TargetType = targetType;
        }
    }

    /// <summary>
    /// Raised for errors during the generation of a serialiser.
    /// </summary>
    public sealed class SerialiserGenerationException : Exception
    {
        /// <summary>
        /// The type to which the serialiser generation exception relates.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initialises a new serialiser generation exception.
        /// </summary>
        /// <param name="errors">Messages explaining the causes of the exception.</param>
        /// <param name="targetType">The type to which the serialiser generation exception relates.</param>
        public SerialiserGenerationException(IReadOnlyList<string> errors, Type targetType)
            : base(DasherExceptionUtil.CreateMessageFromErrors("Cannot generate serialiser for type", errors, targetType))
        {
            TargetType = targetType;
        }
    }

    /// <summary>
    /// Raised for errors during deserialisation.
    /// </summary>
    public sealed class DeserialisationException : Exception
    {
        /// <summary>
        /// The type to which the deserialisation exception relates.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initialises a new deserialisation exception.
        /// </summary>
        /// <param name="message">A message explaining the cause of the exception.</param>
        /// <param name="targetType">The type to which the deserialisation exception relates.</param>
        public DeserialisationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }

    /// <summary>
    /// Raised for errors during serialisation.
    /// </summary>
    public sealed class SerialisationException : Exception
    {
        /// <summary>
        /// The type to which the serialisation exception relates.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initialises a new serialisation exception.
        /// </summary>
        /// <param name="message">A message explaining the cause of the exception.</param>
        /// <param name="targetType">The type to which the serialisation exception relates.</param>
        public SerialisationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }

    internal static class DasherExceptionUtil
    {
        public static string CreateMessageFromErrors(string headline, IReadOnlyList<string> errors, Type type)
        {
            var message = new StringBuilder();
            if (errors.Count == 1)
            {
                message.Append($"{headline} \"{type.Namespace}.{type.Name}\": {errors[0]}");
            }
            else
            {
                message.Append($"{headline} \"{type.Namespace}.{type.Name}\":");
                foreach (var error in errors)
                    message.AppendLine().Append($"- {error}");
            }
            return message.ToString();
        }
    }
}