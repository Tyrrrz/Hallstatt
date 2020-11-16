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
    /// <summary>
    /// Hallstatt adapter for `dotnet test` and IDE integration.
    /// </summary>
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(UriRaw)]
    [ExtensionUri(UriRaw)]
    public partial class TestExecutor : ITestDiscoverer, ITestExecutor, IDisposable
    {
        private const string UriRaw = "executor://tyrrrz/hallstatt/v1";
        private static readonly Uri Uri = new Uri(UriRaw);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private void DiscoverTests(
            string source,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            logger.SendMessage(
                TestMessageLevel.Informational,
                $"Discovering tests in '{source}'."
            );

            foreach (var test in LoadTestsFromAssembly(source))
            {
                _cts.Token.ThrowIfCancellationRequested();

                var testCase = CreateTestCase(test, source);
                discoverySink.SendTestCase(testCase);
            }
        }

        /// <inheritdoc />
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            foreach (var source in sources)
            {
                _cts.Token.ThrowIfCancellationRequested();

                DiscoverTests(source, discoveryContext, logger, discoverySink);
            }
        }

        private void RunTests(
            string source,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(
                TestMessageLevel.Informational,
                $"Running tests in '{source}'."
            );

            foreach (var test in LoadTestsFromAssembly(source))
            {
                _cts.Token.ThrowIfCancellationRequested();

                var testCase = CreateTestCase(test, source);

                frameworkHandle.SendMessage(
                    TestMessageLevel.Informational,
                    $"Running test '{test.Title}' ({testCase.Id}) from assembly '{source}'."
                );

                var testResult = new TestResult(testCase)
                {
                    DisplayName = testCase.DisplayName,
                    Outcome = TestOutcome.None,
                    ComputerName = Environment.MachineName,
                    StartTime = DateTimeOffset.Now
                };

                try
                {
                    test.ExecuteAsync().GetAwaiter().GetResult();

                    testResult.Outcome = TestOutcome.Passed;
                }
                catch (TestSkippedException ex)
                {
                    testResult.Outcome = TestOutcome.Skipped;
                    testResult.ErrorMessage = ex.Message;
                    testResult.ErrorStackTrace = ex.StackTrace;
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
        }

        /// <inheritdoc />
        public void RunTests(
            IEnumerable<string> sources,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            foreach (var source in sources)
            {
                _cts.Token.ThrowIfCancellationRequested();

                RunTests(source, runContext, frameworkHandle);
            }
        }

        /// <inheritdoc />
        public void RunTests(
            IEnumerable<TestCase> testCases,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            foreach (var testCase in testCases)
            {
                _cts.Token.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(testCase.Source))
                {
                    frameworkHandle.SendMessage(
                        TestMessageLevel.Error,
                        $"Test case {testCase.Id} / '{testCase.FullyQualifiedName}' is missing source information."
                    );

                    continue;
                }

                RunTests(testCase.Source, runContext, frameworkHandle);
            }
        }

        /// <inheritdoc />
        public void Cancel() => _cts.Cancel();

        /// <inheritdoc />
        public void Dispose() => _cts.Dispose();
    }

    public partial class TestExecutor
    {
        private static IReadOnlyList<Test> LoadTestsFromAssembly(string source)
        {
            TestController.Clear();

            var assembly = Assembly.LoadFile(source);
            if (assembly == null)
            {
                throw new InvalidOperationException(
                    $"Attempt to load assembly '{source}' returned null."
                );
            }

            var entryPoint = assembly.EntryPoint;
            if (entryPoint == null)
            {
                throw new InvalidOperationException(
                    $"Test assembly '{source}' must define a valid entry point."
                );
            }

            // Entry point has an optional parameter that corresponds to command line arguments
            var parameters = entryPoint.GetParameters().Any()
                ? new object[] {Array.Empty<string>()}
                : Array.Empty<object>();

            entryPoint.Invoke(null, parameters);

            return TestController.GetRegisteredTests(source);
        }

        private static TestCase CreateTestCase(Test test, string source)
        {
            var testCase = new TestCase
            {
                ExecutorUri = Uri,
                FullyQualifiedName = test.Title,
                DisplayName = test.Title,
                Source = source
            };

            testCase.Traits.AddRange(test.Traits.Select(t => new Trait(t.Key, t.Value)));

            return testCase;
        }
    }
}