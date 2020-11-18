using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hallstatt
{
    /// <summary>
    /// Thrown when a test is skipped.
    /// </summary>
    public class TestSkippedException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="TestSkippedException"/>.
        /// </summary>
        public TestSkippedException(string message)
            : base(message) {}
    }

    /// <summary>
    /// Keeps track of test registrations.
    /// </summary>
    public static class TestController
    {
        // Try to avoid duplicates by using delegate references as primary key
        private static readonly HashSet<Delegate> Delegates = new HashSet<Delegate>();
        private static readonly List<Test> Tests = new List<Test>();

        private static readonly IReadOnlyDictionary<string, string?> EmptyTraits =
            new Dictionary<string, string?>(StringComparer.Ordinal);

        /// <summary>
        /// Get registered tests.
        /// </summary>
        public static IReadOnlyList<Test> GetRegisteredTests() => Tests;

        /// <summary>
        /// Get registered tests in assembly.
        /// </summary>
        public static IReadOnlyList<Test> GetRegisteredTests(string assemblyLocation) =>
            GetRegisteredTests()
                .Where(t => string.Equals(t.Assembly.Location, assemblyLocation, StringComparison.OrdinalIgnoreCase))
                .ToArray();

        /// <summary>
        /// Clears all registered tests.
        /// </summary>
        public static void Clear()
        {
            Delegates.Clear();
            Tests.Clear();
        }

        /// <summary>
        /// Registers a test.
        /// </summary>
        public static void Test(string title, Action<TestMetadataConfigurator> configure, Func<ValueTask> executeAsync)
        {
            if (Delegates.Add(executeAsync))
            {
                var metadata = new TestMetadataConfigurator();
                configure(metadata);

                Tests.Add(new Test(
                    title,
                    Assembly.GetCallingAssembly(),
                    metadata.GetTraits(),
                    executeAsync
                ));
            }
        }

        /// <summary>
        /// Registers a test.
        /// </summary>
        public static void Test(string title, Func<ValueTask> executeAsync)
        {
            if (Delegates.Add(executeAsync))
            {
                Tests.Add(new Test(
                    title,
                    Assembly.GetCallingAssembly(),
                    EmptyTraits,
                    executeAsync
                ));
            }
        }

        /// <summary>
        /// Registers a test.
        /// </summary>
        public static void Test(string title, Action<TestMetadataConfigurator> configure, Action execute)
        {
            if (Delegates.Add(execute))
            {
                var metadata = new TestMetadataConfigurator();
                configure(metadata);

                Tests.Add(new Test(
                    title,
                    Assembly.GetCallingAssembly(),
                    metadata.GetTraits(),
                    () =>
                    {
                        execute();
                        return default;
                    }
                ));
            }
        }

        /// <summary>
        /// Registers a test.
        /// </summary>
        public static void Test(string title, Action execute)
        {
            if (Delegates.Add(execute))
            {
                Tests.Add(new Test(
                    title,
                    Assembly.GetCallingAssembly(),
                    EmptyTraits,
                    () =>
                    {
                        execute();
                        return default;
                    }
                ));
            }
        }

        /// <summary>
        /// Registers parametrized tests.
        /// </summary>
        public static void TestMany<TParam>(
            IEnumerable<TParam> parameters,
            Action<TParam, TestMetadataConfigurator> configure,
            Func<TParam, string> getTitle,
            Func<TParam, ValueTask> executeAsync)
        {
            if (Delegates.Add(executeAsync))
            {
                foreach (var p in parameters)
                {
                    var parameter = p;

                    var metadata = new TestMetadataConfigurator();
                    configure(parameter, metadata);

                    var title = getTitle(parameter);

                    Tests.Add(new Test(
                        title,
                        Assembly.GetCallingAssembly(),
                        metadata.GetTraits(),
                        () => executeAsync(parameter)
                    ));
                }
            }
        }

        /// <summary>
        /// Registers parametrized tests.
        /// </summary>
        public static void TestMany<TParam>(
            IEnumerable<TParam> parameters,
            Func<TParam, string> getTitle,
            Func<TParam, ValueTask> executeAsync)
        {
            if (Delegates.Add(executeAsync))
            {
                foreach (var p in parameters)
                {
                    var parameter = p;
                    var title = getTitle(parameter);

                    Tests.Add(new Test(
                        title,
                        Assembly.GetCallingAssembly(),
                        EmptyTraits,
                        () => executeAsync(parameter)
                    ));
                }
            }
        }

        /// <summary>
        /// Registers parametrized tests.
        /// </summary>
        public static void TestMany<TParam>(
            IEnumerable<TParam> parameters,
            Action<TParam, TestMetadataConfigurator> configure,
            Func<TParam, string> getTitle,
            Action<TParam> execute)
        {
            if (Delegates.Add(execute))
            {
                foreach (var p in parameters)
                {
                    var parameter = p;

                    var metadata = new TestMetadataConfigurator();
                    configure(parameter, metadata);

                    var title = getTitle(parameter);

                    Tests.Add(new Test(
                        title,
                        Assembly.GetCallingAssembly(),
                        metadata.GetTraits(),
                        () =>
                        {
                            execute(parameter);
                            return default;
                        }
                    ));
                }
            }
        }

        /// <summary>
        /// Registers parametrized tests.
        /// </summary>
        public static void TestMany<TParam>(
            IEnumerable<TParam> parameters,
            Func<TParam, string> getTitle,
            Action<TParam> execute)
        {
            if (Delegates.Add(execute))
            {
                foreach (var p in parameters)
                {
                    var parameter = p;
                    var title = getTitle(parameter);

                    Tests.Add(new Test(
                        title,
                        Assembly.GetCallingAssembly(),
                        EmptyTraits,
                        () =>
                        {
                            execute(parameter);
                            return default;
                        }
                    ));
                }
            }
        }

        /// <summary>
        /// Signals that the current test should be skipped if the condition evaluates to <code>true</code>.
        /// This method should only be used within the body of a test.
        /// </summary>
        public static void SkipIf(bool shouldSkip, string reason = "No reason specified")
        {
            if (shouldSkip)
            {
                throw new TestSkippedException(reason);
            }
        }

        /// <summary>
        /// Signals that the current test should be skipped.
        /// This method should only be used within the body of a test.
        /// </summary>
        public static void Skip(string reason = "No reason specified") =>
            SkipIf(true, reason);
    }
}