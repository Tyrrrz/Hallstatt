using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hallstatt.Utils;

namespace Hallstatt;

/// <summary>
/// Keeps track of test registrations.
/// </summary>
public static class TestController
{
    private static readonly List<Test> Tests = new();

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
    public static void Clear() => Tests.Clear();

    private static void Test(
        string title,
        Assembly assembly,
        Action<TestMetadataConfigurator>? configure,
        Func<ValueTask> executeAsync)
    {
        var metadata = new TestMetadataConfigurator();
        configure?.Invoke(metadata);

        Tests.Add(new Test(
            Guid.NewGuid(),
            title,
            assembly,
            metadata.GetTraits(),
            metadata.GetIsParallel(),
            metadata.GetIsSkipped(),
            executeAsync
        ));
    }

    /// <summary>
    /// Registers a test.
    /// </summary>
    public static void Test(
        string title,
        Action<TestMetadataConfigurator> configure,
        Func<ValueTask> executeAsync) =>
        Test(title, Assembly.GetCallingAssembly(), configure, executeAsync);

    /// <summary>
    /// Registers a test.
    /// </summary>
    public static void Test(string title, Func<ValueTask> executeAsync) =>
        Test(title, Assembly.GetCallingAssembly(), null, executeAsync);

    /// <summary>
    /// Registers a test.
    /// </summary>
    public static void Test(
        string title,
        Action<TestMetadataConfigurator> configure,
        Action execute) =>
        Test(title, Assembly.GetCallingAssembly(), configure, execute.ToValueTaskFunc());

    /// <summary>
    /// Registers a test.
    /// </summary>
    public static void Test(string title, Action execute) =>
        Test(title, Assembly.GetCallingAssembly(), null, execute.ToValueTaskFunc());

    private static void TestMany<TParam>(
        Assembly assembly,
        IEnumerable<TParam> parameters,
        Action<TParam, TestMetadataConfigurator>? configure,
        Func<TParam, string> getTitle,
        Func<TParam, ValueTask> executeAsync)
    {
        foreach (var p in parameters)
        {
            var parameter = p;

            var metadata = new TestMetadataConfigurator();
            configure?.Invoke(parameter, metadata);

            var title = getTitle(parameter);

            Tests.Add(new Test(
                Guid.NewGuid(),
                title,
                assembly,
                metadata.GetTraits(),
                metadata.GetIsParallel(),
                metadata.GetIsSkipped(),
                () => executeAsync(parameter)
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
        Func<TParam, ValueTask> executeAsync) =>
        TestMany(Assembly.GetCallingAssembly(), parameters, configure, getTitle, executeAsync);

    /// <summary>
    /// Registers parametrized tests.
    /// </summary>
    public static void TestMany<TParam>(
        IEnumerable<TParam> parameters,
        Func<TParam, string> getTitle,
        Func<TParam, ValueTask> executeAsync) =>
        TestMany(Assembly.GetCallingAssembly(), parameters, null, getTitle, executeAsync);

    /// <summary>
    /// Registers parametrized tests.
    /// </summary>
    public static void TestMany<TParam>(
        IEnumerable<TParam> parameters,
        Action<TParam, TestMetadataConfigurator> configure,
        Func<TParam, string> getTitle,
        Action<TParam> execute) =>
        TestMany(Assembly.GetCallingAssembly(), parameters, configure, getTitle, execute.ToValueTaskFunc());

    /// <summary>
    /// Registers parametrized tests.
    /// </summary>
    public static void TestMany<TParam>(
        IEnumerable<TParam> parameters,
        Func<TParam, string> getTitle,
        Action<TParam> execute) =>
        TestMany(Assembly.GetCallingAssembly(), parameters, null, getTitle, execute.ToValueTaskFunc());
}