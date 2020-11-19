using System;
using System.Threading.Tasks;

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
        public AssertionException(string message = "")
            : base(message) {}
    }

    /// <summary>
    /// Assertion utilities.
    /// </summary>
    /// <remarks>
    /// This assertion module is intentionally very basic.
    /// It's recommended to use a more advanced assertion library, such as FluentAssertions or Shouldly.
    /// </remarks>
    public static class Assert
    {
        /// <summary>
        /// Throws an assertion exception.
        /// </summary>
        public static void Fail(string message) => throw new AssertionException(message);

        /// <summary>
        /// Asserts that the specified condition is <code>true</code>.
        /// </summary>
        public static void That(bool condition, string message = "Assertion failed.")
        {
            if (!condition)
                Fail(message);
        }

        /// <summary>
        /// Asserts that the delegate throws the specified exception.
        /// </summary>
        public static TException Throws<TException>(
            Action action,
            bool includeDerived = false)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex) when (includeDerived || ex.GetType() == typeof(TException))
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Expected '{typeof(TException)}' but '{ex.GetType()}' was thrown.");
            }

            throw new AssertionException($"Expected '{typeof(TException)}' but no exception was thrown.");
        }

        /// <summary>
        /// Asserts that the delegate throws the specified exception.
        /// </summary>
        public static async Task<TException> ThrowsAsync<TException>(
            Func<Task> action,
            bool includeDerived = false)
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException ex) when (includeDerived || ex.GetType() == typeof(TException))
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Expected '{typeof(TException)}' but '{ex.GetType()}' was thrown.");
            }

            throw new AssertionException($"Expected '{typeof(TException)}' but no exception was thrown.");
        }
    }
}