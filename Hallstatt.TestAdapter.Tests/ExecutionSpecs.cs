using System;
using System.Linq;
using FluentAssertions;
using Hallstatt.Assertions;
using Hallstatt.TestAdapter.Tests.Fakes;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace Hallstatt.TestAdapter.Tests;

public class ExecutionSpecs
{
    public ExecutionSpecs() => TestController.Clear();

    [Fact]
    public void Passed_tests_are_reported_correctly()
    {
        // Arrange
        TestController.Test(
            "Test 1",
            () => { }
        );

        TestController.Test(
            "Test 2",
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
        testResults.Select(r => r.DisplayName).Should().BeEquivalentTo("Test 1", "Test 2");
        testResults.Select(r => r.Outcome).Should().AllBeEquivalentTo(TestOutcome.Passed);
    }

    [Fact]
    public void Failed_tests_are_reported_correctly()
    {
        // Arrange
        TestController.Test(
            "Test 1",
            () => throw new InvalidOperationException()
        );

        TestController.Test(
            "Test 2",
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
        testResults.Select(r => r.DisplayName).Should().BeEquivalentTo("Test 1", "Test 2");
        testResults.Select(r => r.Outcome).Should().AllBeEquivalentTo(TestOutcome.Failed);
    }

    [Fact]
    public void Skipped_tests_are_reported_correctly()
    {
        // Arrange
        TestController.Test(
            "Test 1",
            o => o.Skip(),
            () => { }
        );

        TestController.Test(
            "Test 2",
            o => o.Skip(),
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
        testResults.Select(r => r.DisplayName).Should().BeEquivalentTo("Test 1", "Test 2");
        testResults.Select(r => r.Outcome).Should().AllBeEquivalentTo(TestOutcome.Skipped);
    }

    [Fact]
    public void Mixed_result_tests_are_reported_correctly()
    {
        // Arrange
        TestController.Test(
            "Test 1",
            () => throw new AssertionException()
        );

        TestController.Test(
            "Test 2",
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
        testResults.Select(r => r.DisplayName).Should().BeEquivalentTo("Test 1", "Test 2");
        testResults.Select(r => r.Outcome).Should().BeEquivalentTo(new[] { TestOutcome.Failed, TestOutcome.Passed });
    }
}