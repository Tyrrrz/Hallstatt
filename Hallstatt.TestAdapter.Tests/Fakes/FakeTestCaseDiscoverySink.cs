using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Hallstatt.TestAdapter.Tests.Fakes
{
    public class FakeTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        private readonly List<TestCase> _testCases = new List<TestCase>();

        public void SendTestCase(TestCase discoveredTest) =>
            _testCases.Add(discoveredTest);

        public IReadOnlyList<TestCase> GetTestCases() => _testCases.ToArray();
    }
}