using System;
using FluentAssertions;
using Hallstatt.Assertions;
using Hallstatt.TestAdapter.Tests.Fakes;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace Hallstatt.TestAdapter.Tests
{
    public class ExecutionSpecs
    {
        public ExecutionSpecs() => TestController.Clear();

        [Fact]
        public void Passed_tests_are_reported_correctly()
        {
            // Arrange
            TestController.Test(
                "Test 1",
                o => o.Trait("foo", "bar"),
                () => { }
            );

            TestController.Test(
                "Test 2",
                o => o.Trait("foo", "baz"),
                () => { }
            );

            var bridge = new TestExecutor();

            var tests = TestController.GetRegisteredTests();
            var runContext = new RunContext();
            var frameworkHandle = new FakeFrameworkHandle();

            // Act
            bridge.RunTests(tests, runContext, frameworkHandle);

            var testResults = frameworkHandle.GetTestResults();

            // Assert
            testResults.Should().HaveCount(2);

            testResults[0].DisplayName.Should().Be("Test 1");
            testResults[0].Traits.Should().BeEquivalentTo(new Trait("foo", "bar"));
            testResults[0].Outcome.Should().Be(TestOutcome.Passed);

            testResults[1].DisplayName.Should().Be("Test 2");
            testResults[1].Traits.Should().BeEquivalentTo(new Trait("foo", "baz"));
            testResults[1].Outcome.Should().Be(TestOutcome.Passed);
        }

        [Fact]
        public void Failed_tests_are_reported_correctly()
        {
            // Arrange
            TestController.Test(
                "Test 1",
                o => o.Trait("foo", "bar"),
                () => throw new InvalidOperationException()
            );

            TestController.Test(
                "Test 2",
                o => o.Trait("foo", "baz"),
                () => throw new AssertionException()
            );

            var bridge = new TestExecutor();

            var tests = TestController.GetRegisteredTests();
            var runContext = new RunContext();
            var frameworkHandle = new FakeFrameworkHandle();

            // Act
            bridge.RunTests(tests, runContext, frameworkHandle);

            var testResults = frameworkHandle.GetTestResults();

            // Assert
            testResults.Should().HaveCount(2);

            testResults[0].DisplayName.Should().Be("Test 1");
            testResults[0].Traits.Should().BeEquivalentTo(new Trait("foo", "bar"));
            testResults[0].Outcome.Should().Be(TestOutcome.Failed);

            testResults[1].DisplayName.Should().Be("Test 2");
            testResults[1].Traits.Should().BeEquivalentTo(new Trait("foo", "baz"));
            testResults[1].Outcome.Should().Be(TestOutcome.Failed);
        }

        [Fact]
        public void Skipped_tests_are_reported_correctly()
        {
            // Arrange
            TestController.Test(
                "Test 1",
                o => o.Skip().Trait("foo", "bar"),
                () => { }
            );

            TestController.Test(
                "Test 2",
                o => o.Skip().Trait("foo", "baz"),
                () => { }
            );

            var bridge = new TestExecutor();

            var tests = TestController.GetRegisteredTests();
            var runContext = new RunContext();
            var frameworkHandle = new FakeFrameworkHandle();

            // Act
            bridge.RunTests(tests, runContext, frameworkHandle);

            var testResults = frameworkHandle.GetTestResults();

            // Assert
            testResults.Should().HaveCount(2);

            testResults[0].DisplayName.Should().Be("Test 1");
            testResults[0].Traits.Should().BeEquivalentTo(new Trait("foo", "bar"));
            testResults[0].Outcome.Should().Be(TestOutcome.Skipped);

            testResults[1].DisplayName.Should().Be("Test 2");
            testResults[1].Traits.Should().BeEquivalentTo(new Trait("foo", "baz"));
            testResults[1].Outcome.Should().Be(TestOutcome.Skipped);
        }

        [Fact]
        public void Mixed_tests_are_reported_correctly()
        {
            // Arrange
            TestController.Test(
                "Test 1",
                o => o.Trait("foo", "bar"),
                () => throw new AssertionException()
            );

            TestController.Test(
                "Test 2",
                o => o.Trait("foo", "baz"),
                () => { }
            );

            var bridge = new TestExecutor();

            var tests = TestController.GetRegisteredTests();
            var runContext = new RunContext();
            var frameworkHandle = new FakeFrameworkHandle();

            // Act
            bridge.RunTests(tests, runContext, frameworkHandle);

            var testResults = frameworkHandle.GetTestResults();

            // Assert
            testResults.Should().HaveCount(2);

            testResults[0].DisplayName.Should().Be("Test 1");
            testResults[0].Traits.Should().BeEquivalentTo(new Trait("foo", "bar"));
            testResults[0].Outcome.Should().Be(TestOutcome.Failed);

            testResults[1].DisplayName.Should().Be("Test 2");
            testResults[1].Traits.Should().BeEquivalentTo(new Trait("foo", "baz"));
            testResults[1].Outcome.Should().Be(TestOutcome.Passed);
        }
    }
}