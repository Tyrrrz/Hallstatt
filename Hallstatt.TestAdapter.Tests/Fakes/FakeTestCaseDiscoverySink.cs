using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Hallstatt.TestAdapter.Tests.Fakes
{
    public class FakeTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        private readonly ConcurrentBag<TestCase> _testCases = new();

        public void SendTestCase(TestCase discoveredTest) =>
            _testCases.Add(discoveredTest);

        public IReadOnlyList<TestCase> GetTestCases() => _testCases.ToArray();
    }
}