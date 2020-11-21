using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Hallstatt.TestAdapter.Tests.Fakes
{
    public class FakeFrameworkHandle : FakeMessageLogger, IFrameworkHandle
    {
        private readonly ConcurrentBag<TestResult> _testResults = new ConcurrentBag<TestResult>();

        public bool EnableShutdownAfterTestRun { get; set; }

        public void RecordResult(TestResult testResult) =>
            _testResults.Add(testResult);

        public void RecordStart(TestCase testCase) => throw new NotSupportedException();

        public void RecordEnd(TestCase testCase, TestOutcome outcome) => throw new NotSupportedException();

        public void RecordAttachments(IList<AttachmentSet> attachmentSets) => throw new NotSupportedException();

        public int LaunchProcessWithDebuggerAttached(
            string filePath,
            string workingDirectory,
            string arguments,
            IDictionary<string, string> environmentVariables) => throw new NotSupportedException();

        public IReadOnlyList<TestResult> GetTestResults() => _testResults.ToArray();
    }
}