using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Hallstatt.TestAdapter
{
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(UriRaw)]
    [ExtensionUri(UriRaw)]
    public partial class TestExecutor : ITestDiscoverer, ITestExecutor, IDisposable
    {
        private const string UriRaw = "executor://tyrrrz/hallstatt/v1";
        private static readonly Uri Uri = new(UriRaw);

        private readonly CancellationTokenSource _cts = new();

        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            foreach (var source in sources)
            {
                _cts.Token.ThrowIfCancellationRequested();

                foreach (var test in LoadTestsFromAssembly(source))
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    discoverySink.SendTestCase(CreateTestCase(test, source));
                }
            }
        }

        private void RunTest(
            Test test,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(
                TestMessageLevel.Informational,
                $"Running test '{test.Title}' ({test.Id}) from assembly '{test.Assembly.FullName}'."
            );

            var testResult = CreateTestResult(test, test.Assembly.Location);
            testResult.StartTime = DateTimeOffset.Now;

            try
            {
                if (!test.IsSkipped)
                {
                    test.ExecuteAsync().GetAwaiter().GetResult();

                    testResult.Outcome = TestOutcome.Passed;
                }
                else
                {
                    testResult.Outcome = TestOutcome.Skipped;
                }
            }
            catch (Exception ex)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = ex.Message;
                testResult.ErrorStackTrace = ex.StackTrace;
            }
            finally
            {
                testResult.EndTime = DateTimeOffset.Now;
                testResult.Duration = testResult.EndTime - testResult.StartTime;
            }

            frameworkHandle.RecordResult(testResult);
        }

        // Accessed from tests
        internal void RunTests(
            IReadOnlyList<Test> tests,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            var concurrentTests = tests.Where(t => t.IsParallel).ToArray();
            var nonConcurrentTests = tests.Where(t => !t.IsParallel).ToArray();

            concurrentTests.AsParallel().WithCancellation(_cts.Token).ForAll(test =>
            {
                _cts.Token.ThrowIfCancellationRequested();
                RunTest(test, runContext, frameworkHandle);
            });

            foreach (var test in nonConcurrentTests)
            {
                _cts.Token.ThrowIfCancellationRequested();
                RunTest(test, runContext, frameworkHandle);
            }
        }

        public void RunTests(
            IEnumerable<string> sources,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            foreach (var source in sources)
            {
                _cts.Token.ThrowIfCancellationRequested();

                var tests = LoadTestsFromAssembly(source);
                RunTests(tests, runContext, frameworkHandle);
            }
        }

        public void RunTests(
            IEnumerable<TestCase> testCases,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            foreach (var sourceGroup in testCases.GroupBy(tc => tc.Source, StringComparer.OrdinalIgnoreCase))
            {
                _cts.Token.ThrowIfCancellationRequested();

                var source = sourceGroup.Key;
                var testIds = sourceGroup.Select(tc => tc.Id).ToHashSet();
                var tests = LoadTestsFromAssembly(source).Where(t => testIds.Contains(t.Id)).ToArray();

                RunTests(tests, runContext, frameworkHandle);
            }
        }

        public void Cancel() => _cts.Cancel();

        public void Dispose() => _cts.Dispose();
    }

    public partial class TestExecutor
    {
        private static IReadOnlyList<Test> LoadTestsFromAssembly(string source)
        {
            TestController.Clear();

            var assembly =
                Assembly.LoadFile(source) ??
                throw new InvalidOperationException($"Attempt to load assembly '{source}' returned null.");

            var entryPoint =
                assembly.EntryPoint ??
                throw new InvalidOperationException($"Test assembly '{source}' must define a valid entry point.");

            // Entry point has an optional parameter that corresponds to command line arguments
            var parameters = entryPoint.GetParameters().Any()
                ? new object[] { Array.Empty<string>() }
                : Array.Empty<object>();

            entryPoint.Invoke(null, parameters);

            return TestController.GetRegisteredTests(source);
        }

        private static TestCase CreateTestCase(Test test, string source)
        {
            var testCase = new TestCase
            {
                Id = test.Id,
                ExecutorUri = Uri,
                FullyQualifiedName = test.Title,
                DisplayName = test.Title,
                Source = source
            };

            testCase.Traits.AddRange(test.Traits.Select(t => new Trait(t.Key, t.Value)));

            return testCase;
        }

        private static TestResult CreateTestResult(TestCase testCase)
        {
            var testResult = new TestResult(testCase)
            {
                DisplayName = testCase.DisplayName,
                Outcome = TestOutcome.None,
                ComputerName = Environment.MachineName,
                StartTime = DateTimeOffset.Now
            };

            testResult.Traits.AddRange(testCase.Traits);

            return testResult;
        }

        private static TestResult CreateTestResult(Test test, string source) =>
            CreateTestResult(CreateTestCase(test, source));
    }
}