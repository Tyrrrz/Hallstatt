using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Hallstatt.Internal;

namespace Hallstatt
{
    /// <summary>
    /// Thrown in case of a failed assertion.
    /// </summary>
    public class AssertionException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="AssertionException"/>.
        /// </summary>
        public AssertionException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Assertion utilities.
    /// </summary>
    public class Assert
    {
        /// <summary>
        /// Throws an assertion exception.
        /// </summary>
        public static void Fail(string message) => throw new AssertionException(message);

        /// <summary>
        /// Resolves available assertions for the given object type.
        /// </summary>
        [Pure]
        public static Assertions<T> That<T>(T value) => new Assertions<T>(value);
    }

    /// <summary>
    /// Encapsulates assertions on an object.
    /// </summary>
    public class Assertions<T>
    {
        /// <summary>
        /// Source object.
        /// </summary>
        public T Source { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Assertions{T}"/>.
        /// </summary>
        public Assertions(T source) => Source = source;

        /// <summary>
        /// Asserts that the object is of specified type.
        /// </summary>
        public void Is(Type type)
        {
            var sourceType = Source?.GetType();
            if (sourceType is null || sourceType != type)
                Assert.Fail($"Expected '{Source}' to be of type '{type}'.");
        }

        /// <summary>
        /// Asserts that the object is of specified type.
        /// </summary>
        public void Is<TTarget>() => Is(typeof(TTarget));

        /// <summary>
        /// Asserts that the object can be assigned to specified type.
        /// </summary>
        public void IsAssignableTo(Type type)
        {
            var sourceType = Source?.GetType();
            if (sourceType is null || !sourceType.IsAssignableTo(type))
                Assert.Fail($"Expected '{Source}' to be assignable to type '{type}'.");
        }

        /// <summary>
        /// Asserts that the object can be assigned to specified type.
        /// </summary>
        public void IsAssignableTo<TTarget>() => IsAssignableTo(typeof(TTarget));

        /// <summary>
        /// Asserts that the object is null.
        /// </summary>
        public void IsNull()
        {
            if (!(Source is null))
                Assert.Fail($"Expected '{Source}' to be null.");
        }

        /// <summary>
        /// Asserts that the object is NOT null.
        /// </summary>
        public void IsNotNull()
        {
            if (Source is null)
                Assert.Fail($"Expected '{Source}' to be not null.");
        }

        /// <summary>
        /// Asserts equality.
        /// </summary>
        public void IsEqualTo(T other, IEqualityComparer<T> comparer)
        {
            if (!comparer.Equals(Source, other))
                Assert.Fail($"Expected '{Source}' to be equal to '{other}'.");
        }

        /// <summary>
        /// Asserts equality.
        /// </summary>
        public void IsEqualTo(T other) => IsEqualTo(other, EqualityComparer<T>.Default);

        /// <summary>
        /// Asserts referential equality.
        /// </summary>
        public void IsSameAs(T other)
        {
            if (!ReferenceEquals(Source, other))
                Assert.Fail($"Expected '{Source}' to be the same reference as '{other}'.");
        }
    }

    /// <summary>
    /// Additional scoped assertions.
    /// </summary>
    public static class AssertionExtensions
    {
        /// <summary>
        /// Asserts that a boolean value is true.
        /// </summary>
        public static void IsTrue(this Assertions<bool> assertions) =>
            assertions.IsEqualTo(true);

        /// <summary>
        /// Asserts that a boolean value is false.
        /// </summary>
        public static void IsFalse(this Assertions<bool> assertions) =>
            assertions.IsEqualTo(false);

        /// <summary>
        /// Asserts that a value is greater than specified.
        /// </summary>
        public static void IsGreaterThan<T>(
            this Assertions<T> assertions,
            T value)
            where T : IComparable<T>
        {
            if (assertions.Source.CompareTo(value) <= 0)
                Assert.Fail($"Expected '{assertions.Source}' to be greater than '{value}'.");
        }

        /// <summary>
        /// Asserts that a value is greater than specified.
        /// </summary>
        public static void IsGreaterThanOrEqualTo<T>(
            this Assertions<T> assertions,
            T value)
            where T : IComparable<T>
        {
            if (assertions.Source.CompareTo(value) < 0)
                Assert.Fail($"Expected '{assertions.Source}' to be greater than or equal to '{value}'.");
        }

        /// <summary>
        /// Asserts that a value is less than specified.
        /// </summary>
        public static void IsLessThan<T>(
            this Assertions<T> assertions,
            T value)
            where T : IComparable<T>
        {
            if (assertions.Source.CompareTo(value) >= 0)
                Assert.Fail($"Expected '{assertions.Source}' to be less than '{value}'.");
        }

        /// <summary>
        /// Asserts that a value is less than specified.
        /// </summary>
        public static void IsLessThanOrEqualTo<T>(
            this Assertions<T> assertions,
            T value)
            where T : IComparable<T>
        {
            if (assertions.Source.CompareTo(value) > 0)
                Assert.Fail($"Expected '{assertions.Source}' to be less than or equal to '{value}'.");
        }

        /// <summary>
        /// Asserts that a string is null or empty.
        /// </summary>
        public static void IsNullOrEmpty(this Assertions<string> assertions)
        {
            if (!string.IsNullOrEmpty(assertions.Source))
                Assert.Fail($"Expected '{assertions.Source}' to be null or empty.");
        }

        /// <summary>
        /// Asserts that a string is NOT null or empty.
        /// </summary>
        public static void IsNotNullOrEmpty(this Assertions<string> assertions)
        {
            if (string.IsNullOrEmpty(assertions.Source))
                Assert.Fail($"Expected '{assertions.Source}' to not be null or empty.");
        }

        /// <summary>
        /// Asserts that a string is null or white space.
        /// </summary>
        public static void IsNullOrWhiteSpace(this Assertions<string> assertions)
        {
            if (!string.IsNullOrWhiteSpace(assertions.Source))
                Assert.Fail($"Expected '{assertions.Source}' to be null or white space.");
        }

        /// <summary>
        /// Asserts that a string is NOT null or white space.
        /// </summary>
        public static void IsNotNullOrWhiteSpace(this Assertions<string> assertions)
        {
            if (string.IsNullOrWhiteSpace(assertions.Source))
                Assert.Fail($"Expected '{assertions.Source}' to not be null or white space.");
        }

        /// <summary>
        /// Asserts that a file or directory exists.
        /// </summary>
        public static void Exists(this Assertions<FileSystemInfo> assertions)
        {
            if (!assertions.Source.Exists)
                Assert.Fail($"Expected path '{assertions.Source}' to exist.");
        }

        /// <summary>
        /// Asserts that the delegate throws.
        /// </summary>
        public static TException Throws<TException>(
            this Assertions<Action> assertions,
            bool includeDerived = false)
            where TException : Exception
        {
            try
            {
                assertions.Source();
            }
            catch (TException ex) when (includeDerived || ex.GetType() == typeof(TException))
            {
                return ex;
            }

            throw new AssertionException($"Expected '{typeof(TException)}' but no exception was thrown.");
        }

        /// <summary>
        /// Asserts that the delegate throws.
        /// </summary>
        public static async ValueTask<TException> ThrowsAsync<TException>(
            this Assertions<Func<ValueTask>> assertions,
            bool includeDerived = false)
            where TException : Exception
        {
            try
            {
                await assertions.Source();
            }
            catch (TException ex) when (includeDerived || ex.GetType() == typeof(TException))
            {
                return ex;
            }

            throw new AssertionException($"Expected '{typeof(TException)}' but no exception was thrown.");
        }

        /// <summary>
        /// Asserts that the delegate throws.
        /// </summary>
        public static async Task<TException> ThrowsAsync<TException>(
            this Assertions<Func<Task>> assertions,
            bool includeDerived = false)
            where TException : Exception
        {
            try
            {
                await assertions.Source();
            }
            catch (TException ex) when (includeDerived || ex.GetType() == typeof(TException))
            {
                return ex;
            }

            throw new AssertionException($"Expected '{typeof(TException)}' but no exception was thrown.");
        }
    }
}