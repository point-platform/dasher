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
using System.Reflection;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeProtected.Global

namespace Dasher
{
    // NOTE this file is generated

    /// <summary>
    /// Static helper methods for working with unions.
    /// </summary>
    public static class Union
    {
        /// <summary>
        /// Gets whether <paramref name="type"/> is one of the family of generic union types.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is a union type, otherwise <c>false</c>.</returns>
        public static bool IsUnionType(Type type)
        {
            return type.FullName.StartsWith("Dasher.Union`", StringComparison.Ordinal)
                && ReferenceEquals(type.GetTypeInfo().Assembly, typeof(Union).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Gets the member types of the union represented by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The union type to inspect.</param>
        /// <returns>The list of member types within the union.</returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not a union type.</exception>
        public static IReadOnlyList<Type> GetTypes(Type type)
        {
            if (!IsUnionType(type))
                throw new ArgumentException("Must be a union type.", nameof(type));

            return type.GetGenericArguments();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 2 types.
    /// </summary>
    public abstract class Union<T1, T2>
    {
        /// <summary>
        /// Applies the contained value to one of 2 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2);

        /// <summary>
        /// Invokes one of 2 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2>(T2 value) => new Type2(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2> union) => (T2)union.Value;

        private sealed class Type1 : Union<T1, T2>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 3 types.
    /// </summary>
    public abstract class Union<T1, T2, T3>
    {
        /// <summary>
        /// Applies the contained value to one of 3 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3);

        /// <summary>
        /// Invokes one of 3 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3>(T3 value) => new Type3(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3> union) => (T3)union.Value;

        private sealed class Type1 : Union<T1, T2, T3>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 4 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4>
    {
        /// <summary>
        /// Applies the contained value to one of 4 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4);

        /// <summary>
        /// Invokes one of 4 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4>(T4 value) => new Type4(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4> union) => (T4)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 5 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Applies the contained value to one of 5 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5);

        /// <summary>
        /// Invokes one of 5 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5>(T5 value) => new Type5(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5> union) => (T5)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 6 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6>
    {
        /// <summary>
        /// Applies the contained value to one of 6 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6);

        /// <summary>
        /// Invokes one of 6 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T6 value) => new Type6(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6> union) => (T6)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 7 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7>
    {
        /// <summary>
        /// Applies the contained value to one of 7 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7);

        /// <summary>
        /// Invokes one of 7 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7>(T7 value) => new Type7(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7> union) => (T7)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 8 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        /// <summary>
        /// Applies the contained value to one of 8 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8);

        /// <summary>
        /// Invokes one of 8 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8>(T8 value) => new Type8(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8> union) => (T8)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 9 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        /// <summary>
        /// Applies the contained value to one of 9 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9);

        /// <summary>
        /// Invokes one of 9 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 value) => new Type9(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9> union) => (T9)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 10 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        /// <summary>
        /// Applies the contained value to one of 10 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10);

        /// <summary>
        /// Invokes one of 10 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T10 value) => new Type10(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> union) => (T10)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 11 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {
        /// <summary>
        /// Applies the contained value to one of 11 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11);

        /// <summary>
        /// Invokes one of 11 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T11 value) => new Type11(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> union) => (T11)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 12 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {
        /// <summary>
        /// Applies the contained value to one of 12 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <param name="func12">A function from <typeparamref name="T12"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12);

        /// <summary>
        /// Invokes one of 12 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        /// <param name="action12">An action accepting <typeparamref name="T12"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T12"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T12"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(T12 value) => new Type12(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            if (value is T12) { union = new Type12((T12)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T11 value) => new Type11(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T12"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T12 value) => new Type12(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T11)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T12"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T12"/>.</exception>
        public static explicit operator T12(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> union) => (T12)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type12 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        {
            private T12 _value;
            public override object Value => _value;
            public override Type Type => typeof(T12);
            public Type12(T12 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12) => func12(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12) => action12(_value);
            public override bool Equals(object o) => o is Type12 ? Equals(((Type12)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 13 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {
        /// <summary>
        /// Applies the contained value to one of 13 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <param name="func12">A function from <typeparamref name="T12"/> to <typeparamref name="T"/>.</param>
        /// <param name="func13">A function from <typeparamref name="T13"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13);

        /// <summary>
        /// Invokes one of 13 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        /// <param name="action12">An action accepting <typeparamref name="T12"/>.</param>
        /// <param name="action13">An action accepting <typeparamref name="T13"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T12"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T12"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T12 value) => new Type12(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T13"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T13"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(T13 value) => new Type13(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            if (value is T12) { union = new Type12((T12)value); return true; }
            if (value is T13) { union = new Type13((T13)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T11 value) => new Type11(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T12"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T12 value) => new Type12(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T13"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T13 value) => new Type13(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T11)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T12"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T12"/>.</exception>
        public static explicit operator T12(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T12)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T13"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T13"/>.</exception>
        public static explicit operator T13(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> union) => (T13)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type12 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T12 _value;
            public override object Value => _value;
            public override Type Type => typeof(T12);
            public Type12(T12 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func12(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action12(_value);
            public override bool Equals(object o) => o is Type12 ? Equals(((Type12)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type13 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        {
            private T13 _value;
            public override object Value => _value;
            public override Type Type => typeof(T13);
            public Type13(T13 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13) => func13(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13) => action13(_value);
            public override bool Equals(object o) => o is Type13 ? Equals(((Type13)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 14 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {
        /// <summary>
        /// Applies the contained value to one of 14 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <param name="func12">A function from <typeparamref name="T12"/> to <typeparamref name="T"/>.</param>
        /// <param name="func13">A function from <typeparamref name="T13"/> to <typeparamref name="T"/>.</param>
        /// <param name="func14">A function from <typeparamref name="T14"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14);

        /// <summary>
        /// Invokes one of 14 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        /// <param name="action12">An action accepting <typeparamref name="T12"/>.</param>
        /// <param name="action13">An action accepting <typeparamref name="T13"/>.</param>
        /// <param name="action14">An action accepting <typeparamref name="T14"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T12"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T12"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T12 value) => new Type12(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T13"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T13"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T13 value) => new Type13(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T14"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T14"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(T14 value) => new Type14(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            if (value is T12) { union = new Type12((T12)value); return true; }
            if (value is T13) { union = new Type13((T13)value); return true; }
            if (value is T14) { union = new Type14((T14)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T11 value) => new Type11(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T12"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T12 value) => new Type12(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T13"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T13 value) => new Type13(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T14"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T14 value) => new Type14(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T11)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T12"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T12"/>.</exception>
        public static explicit operator T12(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T12)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T13"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T13"/>.</exception>
        public static explicit operator T13(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T13)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T14"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T14"/>.</exception>
        public static explicit operator T14(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> union) => (T14)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type12 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T12 _value;
            public override object Value => _value;
            public override Type Type => typeof(T12);
            public Type12(T12 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func12(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action12(_value);
            public override bool Equals(object o) => o is Type12 ? Equals(((Type12)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type13 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T13 _value;
            public override object Value => _value;
            public override Type Type => typeof(T13);
            public Type13(T13 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func13(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action13(_value);
            public override bool Equals(object o) => o is Type13 ? Equals(((Type13)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type14 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        {
            private T14 _value;
            public override object Value => _value;
            public override Type Type => typeof(T14);
            public Type14(T14 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14) => func14(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14) => action14(_value);
            public override bool Equals(object o) => o is Type14 ? Equals(((Type14)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 15 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {
        /// <summary>
        /// Applies the contained value to one of 15 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <param name="func12">A function from <typeparamref name="T12"/> to <typeparamref name="T"/>.</param>
        /// <param name="func13">A function from <typeparamref name="T13"/> to <typeparamref name="T"/>.</param>
        /// <param name="func14">A function from <typeparamref name="T14"/> to <typeparamref name="T"/>.</param>
        /// <param name="func15">A function from <typeparamref name="T15"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15);

        /// <summary>
        /// Invokes one of 15 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        /// <param name="action12">An action accepting <typeparamref name="T12"/>.</param>
        /// <param name="action13">An action accepting <typeparamref name="T13"/>.</param>
        /// <param name="action14">An action accepting <typeparamref name="T14"/>.</param>
        /// <param name="action15">An action accepting <typeparamref name="T15"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T12"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T12"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T12 value) => new Type12(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T13"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T13"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T13 value) => new Type13(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T14"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T14"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T14 value) => new Type14(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T15"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T15"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(T15 value) => new Type15(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            if (value is T12) { union = new Type12((T12)value); return true; }
            if (value is T13) { union = new Type13((T13)value); return true; }
            if (value is T14) { union = new Type14((T14)value); return true; }
            if (value is T15) { union = new Type15((T15)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T11 value) => new Type11(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T12"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T12 value) => new Type12(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T13"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T13 value) => new Type13(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T14"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T14 value) => new Type14(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T15"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T15 value) => new Type15(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T11)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T12"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T12"/>.</exception>
        public static explicit operator T12(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T12)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T13"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T13"/>.</exception>
        public static explicit operator T13(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T13)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T14"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T14"/>.</exception>
        public static explicit operator T14(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T14)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T15"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T15"/>.</exception>
        public static explicit operator T15(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> union) => (T15)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type12 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T12 _value;
            public override object Value => _value;
            public override Type Type => typeof(T12);
            public Type12(T12 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func12(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action12(_value);
            public override bool Equals(object o) => o is Type12 ? Equals(((Type12)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type13 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T13 _value;
            public override object Value => _value;
            public override Type Type => typeof(T13);
            public Type13(T13 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func13(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action13(_value);
            public override bool Equals(object o) => o is Type13 ? Equals(((Type13)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type14 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T14 _value;
            public override object Value => _value;
            public override Type Type => typeof(T14);
            public Type14(T14 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func14(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action14(_value);
            public override bool Equals(object o) => o is Type14 ? Equals(((Type14)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type15 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        {
            private T15 _value;
            public override object Value => _value;
            public override Type Type => typeof(T15);
            public Type15(T15 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15) => func15(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15) => action15(_value);
            public override bool Equals(object o) => o is Type15 ? Equals(((Type15)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }

    /// <summary>
    /// Models a value which may be of one of 16 types.
    /// </summary>
    public abstract class Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {
        /// <summary>
        /// Applies the contained value to one of 16 functions, based upon the type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided functions will be invoked.
        /// </remarks>
        /// <param name="func1">A function from <typeparamref name="T1"/> to <typeparamref name="T"/>.</param>
        /// <param name="func2">A function from <typeparamref name="T2"/> to <typeparamref name="T"/>.</param>
        /// <param name="func3">A function from <typeparamref name="T3"/> to <typeparamref name="T"/>.</param>
        /// <param name="func4">A function from <typeparamref name="T4"/> to <typeparamref name="T"/>.</param>
        /// <param name="func5">A function from <typeparamref name="T5"/> to <typeparamref name="T"/>.</param>
        /// <param name="func6">A function from <typeparamref name="T6"/> to <typeparamref name="T"/>.</param>
        /// <param name="func7">A function from <typeparamref name="T7"/> to <typeparamref name="T"/>.</param>
        /// <param name="func8">A function from <typeparamref name="T8"/> to <typeparamref name="T"/>.</param>
        /// <param name="func9">A function from <typeparamref name="T9"/> to <typeparamref name="T"/>.</param>
        /// <param name="func10">A function from <typeparamref name="T10"/> to <typeparamref name="T"/>.</param>
        /// <param name="func11">A function from <typeparamref name="T11"/> to <typeparamref name="T"/>.</param>
        /// <param name="func12">A function from <typeparamref name="T12"/> to <typeparamref name="T"/>.</param>
        /// <param name="func13">A function from <typeparamref name="T13"/> to <typeparamref name="T"/>.</param>
        /// <param name="func14">A function from <typeparamref name="T14"/> to <typeparamref name="T"/>.</param>
        /// <param name="func15">A function from <typeparamref name="T15"/> to <typeparamref name="T"/>.</param>
        /// <param name="func16">A function from <typeparamref name="T16"/> to <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The value returned from all functions.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public abstract T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16);

        /// <summary>
        /// Invokes one of 16 actions, based upon the contained member type.
        /// </summary>
        /// <remarks>
        /// Only one of the provided actions will be invoked.
        /// </remarks>
        /// <param name="action1">An action accepting <typeparamref name="T1"/>.</param>
        /// <param name="action2">An action accepting <typeparamref name="T2"/>.</param>
        /// <param name="action3">An action accepting <typeparamref name="T3"/>.</param>
        /// <param name="action4">An action accepting <typeparamref name="T4"/>.</param>
        /// <param name="action5">An action accepting <typeparamref name="T5"/>.</param>
        /// <param name="action6">An action accepting <typeparamref name="T6"/>.</param>
        /// <param name="action7">An action accepting <typeparamref name="T7"/>.</param>
        /// <param name="action8">An action accepting <typeparamref name="T8"/>.</param>
        /// <param name="action9">An action accepting <typeparamref name="T9"/>.</param>
        /// <param name="action10">An action accepting <typeparamref name="T10"/>.</param>
        /// <param name="action11">An action accepting <typeparamref name="T11"/>.</param>
        /// <param name="action12">An action accepting <typeparamref name="T12"/>.</param>
        /// <param name="action13">An action accepting <typeparamref name="T13"/>.</param>
        /// <param name="action14">An action accepting <typeparamref name="T14"/>.</param>
        /// <param name="action15">An action accepting <typeparamref name="T15"/>.</param>
        /// <param name="action16">An action accepting <typeparamref name="T16"/>.</param>
        public abstract void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16);

        /// <summary>
        /// Gets the contained member as <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the contained value's type.
        /// </summary>
		/// <remarks>
		/// This type will be one of the union's generic member types.
		/// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T1"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T1 value) => new Type1(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T2"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T2 value) => new Type2(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T3"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T3"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T3 value) => new Type3(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T4"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T4"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T4 value) => new Type4(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T5"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T5"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T5 value) => new Type5(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T6"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T6"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T6 value) => new Type6(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T7"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T7"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T7 value) => new Type7(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T8"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T8"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T8 value) => new Type8(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T9"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T9"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T9 value) => new Type9(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T10"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T10"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T10 value) => new Type10(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T11"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T11"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T11 value) => new Type11(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T12"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T12"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T12 value) => new Type12(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T13"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T13"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T13 value) => new Type13(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T14"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T14"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T14 value) => new Type14(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T15"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T15"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T15 value) => new Type15(value);

        /// <summary>
        /// Instantiates an instance of this union from a value of type <typeparamref name="T16"/>.
        /// </summary>
        /// <param name="value">The value of the returned union.</param>
        /// <returns>A union containing <paramref name="value" /> of type <typeparamref name="T16"/>.</returns>
        public static Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(T16 value) => new Type16(value);

        /// <summary>
        /// Attempts to instantiate an instance of this union from <paramref name="value" />.
        /// </summary>
		/// <remarks>
		/// If the type of <paramref name="value" /> is not one of the union's member types,
		/// instantiation will fail.
		/// </remarks>
        /// <param name="value">The value to attempt instantiation with.</param>
        /// <param name="union">The resulting union if instantiation was successful, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if instantiation was successful, otherwise <c>false</c>.</returns>
        public static bool TryCreate(object value, out Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union)
        {
            if (value is T1) { union = new Type1((T1)value); return true; }
            if (value is T2) { union = new Type2((T2)value); return true; }
            if (value is T3) { union = new Type3((T3)value); return true; }
            if (value is T4) { union = new Type4((T4)value); return true; }
            if (value is T5) { union = new Type5((T5)value); return true; }
            if (value is T6) { union = new Type6((T6)value); return true; }
            if (value is T7) { union = new Type7((T7)value); return true; }
            if (value is T8) { union = new Type8((T8)value); return true; }
            if (value is T9) { union = new Type9((T9)value); return true; }
            if (value is T10) { union = new Type10((T10)value); return true; }
            if (value is T11) { union = new Type11((T11)value); return true; }
            if (value is T12) { union = new Type12((T12)value); return true; }
            if (value is T13) { union = new Type13((T13)value); return true; }
            if (value is T14) { union = new Type14((T14)value); return true; }
            if (value is T15) { union = new Type15((T15)value); return true; }
            if (value is T16) { union = new Type16((T16)value); return true; }
            union = null;
            return false;
        }

        private Union() {}

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T1"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 value) => new Type1(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T2"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T2 value) => new Type2(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T3"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T3 value) => new Type3(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T4"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T4 value) => new Type4(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T5"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T5 value) => new Type5(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T6"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T6 value) => new Type6(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T7"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T7 value) => new Type7(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T8"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T8 value) => new Type8(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T9"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T9 value) => new Type9(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T10"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T10 value) => new Type10(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T11"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T11 value) => new Type11(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T12"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T12 value) => new Type12(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T13"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T13 value) => new Type13(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T14"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T14 value) => new Type14(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T15"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T15 value) => new Type15(value);

        /// <summary>
        /// Implicitly converts values of type <typeparamref name="T16"/> to an instance of this union.
        /// </summary>
        /// <param name="value">The type to implicitly convert.</param>
        /// <returns>The resulting union value.</returns>
        public static implicit operator Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T16 value) => new Type16(value);

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T1"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T1"/>.</exception>
        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T1)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T2"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T2"/>.</exception>
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T2)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T3"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T3"/>.</exception>
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T3)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T4"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T4"/>.</exception>
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T4)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T5"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T5"/>.</exception>
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T5)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T6"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T6"/>.</exception>
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T6)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T7"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T7"/>.</exception>
        public static explicit operator T7(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T7)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T8"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T8"/>.</exception>
        public static explicit operator T8(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T8)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T9"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T9"/>.</exception>
        public static explicit operator T9(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T9)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T10"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T10"/>.</exception>
        public static explicit operator T10(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T10)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T11"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T11"/>.</exception>
        public static explicit operator T11(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T11)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T12"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T12"/>.</exception>
        public static explicit operator T12(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T12)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T13"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T13"/>.</exception>
        public static explicit operator T13(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T13)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T14"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T14"/>.</exception>
        public static explicit operator T14(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T14)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T15"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T15"/>.</exception>
        public static explicit operator T15(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T15)union.Value;

        /// <summary>
        /// Attempts to cast the union's inner value to member type <typeparamref name="T16"/>.
        /// </summary>
		/// <remarks>
		/// Casting is rarely the best approach. For safety and performance, use one of the <c>Match</c> overloads instead.
		/// </remarks>
        /// <param name="union">The union to cast.</param>
        /// <exception cref="InvalidCastException"><paramref name="union"/> does not contain a value of type <typeparamref name="T16"/>.</exception>
        public static explicit operator T16(Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> union) => (T16)union.Value;

        private sealed class Type1 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T1 _value;
            public override object Value => _value;
            public override Type Type => typeof(T1);
            public Type1(T1 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func1(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action1(_value);
            public override bool Equals(object o) => o is Type1 ? Equals(((Type1)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type2 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T2 _value;
            public override object Value => _value;
            public override Type Type => typeof(T2);
            public Type2(T2 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func2(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action2(_value);
            public override bool Equals(object o) => o is Type2 ? Equals(((Type2)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type3 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T3 _value;
            public override object Value => _value;
            public override Type Type => typeof(T3);
            public Type3(T3 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func3(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action3(_value);
            public override bool Equals(object o) => o is Type3 ? Equals(((Type3)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type4 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T4 _value;
            public override object Value => _value;
            public override Type Type => typeof(T4);
            public Type4(T4 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func4(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action4(_value);
            public override bool Equals(object o) => o is Type4 ? Equals(((Type4)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type5 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T5 _value;
            public override object Value => _value;
            public override Type Type => typeof(T5);
            public Type5(T5 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func5(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action5(_value);
            public override bool Equals(object o) => o is Type5 ? Equals(((Type5)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type6 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T6 _value;
            public override object Value => _value;
            public override Type Type => typeof(T6);
            public Type6(T6 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func6(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action6(_value);
            public override bool Equals(object o) => o is Type6 ? Equals(((Type6)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type7 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T7 _value;
            public override object Value => _value;
            public override Type Type => typeof(T7);
            public Type7(T7 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func7(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action7(_value);
            public override bool Equals(object o) => o is Type7 ? Equals(((Type7)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type8 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T8 _value;
            public override object Value => _value;
            public override Type Type => typeof(T8);
            public Type8(T8 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func8(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action8(_value);
            public override bool Equals(object o) => o is Type8 ? Equals(((Type8)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type9 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T9 _value;
            public override object Value => _value;
            public override Type Type => typeof(T9);
            public Type9(T9 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func9(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action9(_value);
            public override bool Equals(object o) => o is Type9 ? Equals(((Type9)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type10 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T10 _value;
            public override object Value => _value;
            public override Type Type => typeof(T10);
            public Type10(T10 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func10(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action10(_value);
            public override bool Equals(object o) => o is Type10 ? Equals(((Type10)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type11 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T11 _value;
            public override object Value => _value;
            public override Type Type => typeof(T11);
            public Type11(T11 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func11(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action11(_value);
            public override bool Equals(object o) => o is Type11 ? Equals(((Type11)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type12 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T12 _value;
            public override object Value => _value;
            public override Type Type => typeof(T12);
            public Type12(T12 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func12(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action12(_value);
            public override bool Equals(object o) => o is Type12 ? Equals(((Type12)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type13 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T13 _value;
            public override object Value => _value;
            public override Type Type => typeof(T13);
            public Type13(T13 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func13(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action13(_value);
            public override bool Equals(object o) => o is Type13 ? Equals(((Type13)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type14 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T14 _value;
            public override object Value => _value;
            public override Type Type => typeof(T14);
            public Type14(T14 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func14(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action14(_value);
            public override bool Equals(object o) => o is Type14 ? Equals(((Type14)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type15 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T15 _value;
            public override object Value => _value;
            public override Type Type => typeof(T15);
            public Type15(T15 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func15(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action15(_value);
            public override bool Equals(object o) => o is Type15 ? Equals(((Type15)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }

        private sealed class Type16 : Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
        {
            private T16 _value;
            public override object Value => _value;
            public override Type Type => typeof(T16);
            public Type16(T16 value) { _value = value; }
            public override T Match<T>(Func<T1, T> func1, Func<T2, T> func2, Func<T3, T> func3, Func<T4, T> func4, Func<T5, T> func5, Func<T6, T> func6, Func<T7, T> func7, Func<T8, T> func8, Func<T9, T> func9, Func<T10, T> func10, Func<T11, T> func11, Func<T12, T> func12, Func<T13, T> func13, Func<T14, T> func14, Func<T15, T> func15, Func<T16, T> func16) => func16(_value);
            public override void Match(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action<T5> action5, Action<T6> action6, Action<T7> action7, Action<T8> action8, Action<T9> action9, Action<T10> action10, Action<T11> action11, Action<T12> action12, Action<T13> action13, Action<T14> action14, Action<T15> action15, Action<T16> action16) => action16(_value);
            public override bool Equals(object o) => o is Type16 ? Equals(((Type16)o)._value, _value) : Equals(o, _value);
            public override int GetHashCode() => _value?.GetHashCode() ?? 0;
            public override string ToString() => _value?.ToString();
        }
    }
}