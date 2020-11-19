using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Hallstatt.Tests
{
    public class RegistrationSpecs
    {
        public RegistrationSpecs() => TestController.Clear();

        [Fact]
        public void I_can_register_a_single_test()
        {
            // Act
            TestController.Test(
                "My test",
                () => { }
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().ContainSingle();
            registeredTests.Should().AllBeEquivalentTo(new Test(
                "My test",
                typeof(RegistrationSpecs).Assembly,
                new Dictionary<string, string?>(),
                () => default
            ));
        }

        [Fact]
        public void I_can_register_a_single_test_with_traits()
        {
            // Act
            TestController.Test(
                "My test",
                o => o.Trait("foo", "bar").Trait("baz"),
                () => { }
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().ContainSingle();
            registeredTests.Should().AllBeEquivalentTo(new Test(
                "My test",
                typeof(RegistrationSpecs).Assembly,
                new Dictionary<string, string?>
                {
                    ["foo"] = "bar",
                    ["baz"] = null
                },
                () => default
            ));
        }

        [Fact]
        public void I_can_register_a_single_async_test_with_traits()
        {
            // Act
            TestController.Test(
                "My test",
                o => o.Trait("foo", "bar").Trait("baz"),
                async () => await Task.Yield()
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().ContainSingle();
            registeredTests.Should().AllBeEquivalentTo(new Test(
                "My test",
                typeof(RegistrationSpecs).Assembly,
                new Dictionary<string, string?>
                {
                    ["foo"] = "bar",
                    ["baz"] = null
                },
                () => default
            ));
        }

        [Fact]
        public async Task I_can_register_a_single_test_and_it_executes_correctly()
        {
            // Arrange
            var hasExecuted = false;

            // Act
            TestController.Test(
                "My test",
                () => hasExecuted = true
            );

            foreach (var test in TestController.GetRegisteredTests())
                await test.ExecuteAsync();

            // Assert
            hasExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task I_can_register_a_single_async_test_and_it_executes_correctly()
        {
            // Arrange
            var hasExecuted = false;

            // Act
            TestController.Test(
                "My test",
                async () =>
                {
                    await Task.Yield();
                    hasExecuted = true;
                }
            );

            foreach (var test in TestController.GetRegisteredTests())
                await test.ExecuteAsync();

            // Assert
            hasExecuted.Should().BeTrue();
        }

        [Fact]
        public void I_can_register_multiple_tests()
        {
            // Act
            TestController.TestMany(
                new[]
                {
                    new {Foo = 1, Bar = 2},
                    new {Foo = 3, Bar = 4},
                    new {Foo = 5, Bar = 6}
                },
                p => $"My test ({p.Foo} {p.Bar})",
                p => { }
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().HaveCount(3);
            registeredTests.Should().BeEquivalentTo(
                new Test(
                    "My test (1 2)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>(),
                    () => default),

                new Test(
                    "My test (3 4)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>(),
                    () => default),

                new Test(
                    "My test (5 6)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>(),
                    () => default)
            );
        }

        [Fact]
        public void I_can_register_multiple_tests_with_traits()
        {
            // Act
            TestController.TestMany(
                new[]
                {
                    new {Foo = 1, Bar = 2},
                    new {Foo = 3, Bar = 4},
                    new {Foo = 5, Bar = 6}
                },
                (p, o) => o.Trait("Foo", p.Foo.ToString()),
                p => $"My test ({p.Foo} {p.Bar})",
                p => { }
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().HaveCount(3);
            registeredTests.Should().BeEquivalentTo(
                new Test(
                    "My test (1 2)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "1"
                    },
                    () => default),

                new Test(
                    "My test (3 4)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "3"
                    },
                    () => default),

                new Test(
                    "My test (5 6)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "5"
                    },
                    () => default)
            );
        }

        [Fact]
        public void I_can_register_multiple_async_tests_with_traits()
        {
            // Act
            TestController.TestMany(
                new[]
                {
                    new {Foo = 1, Bar = 2},
                    new {Foo = 3, Bar = 4},
                    new {Foo = 5, Bar = 6}
                },
                (p, o) => o.Trait("Foo", p.Foo.ToString()),
                p => $"My test ({p.Foo} {p.Bar})",
                async p => await Task.Yield()
            );

            var registeredTests = TestController.GetRegisteredTests();

            // Assert
            registeredTests.Should().HaveCount(3);
            registeredTests.Should().BeEquivalentTo(
                new Test(
                    "My test (1 2)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "1"
                    },
                    () => default),

                new Test(
                    "My test (3 4)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "3"
                    },
                    () => default),

                new Test(
                    "My test (5 6)",
                    typeof(RegistrationSpecs).Assembly,
                    new Dictionary<string, string?>
                    {
                        ["Foo"] = "5"
                    },
                    () => default)
            );
        }

        [Fact]
        public async Task I_can_register_multiple_tests_and_they_execute_correctly()
        {
            // Arrange
            var executeCount = 0;

            // Act
            TestController.TestMany(
                new[]
                {
                    new {Foo = 1, Bar = 2},
                    new {Foo = 3, Bar = 4},
                    new {Foo = 5, Bar = 6}
                },
                p => $"My test ({p.Foo} {p.Bar})",
                p => executeCount++
            );

            foreach (var test in TestController.GetRegisteredTests())
                await test.ExecuteAsync();

            // Assert
            executeCount.Should().Be(3);
        }

        [Fact]
        public async Task I_can_register_multiple_async_tests_and_they_execute_correctly()
        {
            // Arrange
            var executeCount = 0;

            // Act
            TestController.TestMany(
                new[]
                {
                    new {Foo = 1, Bar = 2},
                    new {Foo = 3, Bar = 4},
                    new {Foo = 5, Bar = 6}
                },
                p => $"My test ({p.Foo} {p.Bar})",
                async p =>
                {
                    await Task.Yield();
                    executeCount++;
                });

            foreach (var test in TestController.GetRegisteredTests())
                await test.ExecuteAsync();

            // Assert
            executeCount.Should().Be(3);
        }

        [Fact]
        public async Task I_can_skip_a_test()
        {
            // Arrange
            TestController.Test(
                "My test",
                () => TestController.Skip()
            );

            // Act & assert
            await Xunit.Assert.ThrowsAsync<TestSkippedException>(async () =>
            {
                foreach (var test in TestController.GetRegisteredTests())
                    await test.ExecuteAsync();
            });
        }

        [Fact]
        public async Task I_can_conditionally_skip_a_test_and_it_stops_if_the_condition_is_true()
        {
            // Arrange
            TestController.Test(
                "My test",
                () => TestController.SkipIf(true)
            );

            // Act & assert
            await Xunit.Assert.ThrowsAsync<TestSkippedException>(async () =>
            {
                foreach (var test in TestController.GetRegisteredTests())
                    await test.ExecuteAsync();
            });
        }

        [Fact]
        public async Task I_can_conditionally_skip_a_test_and_it_continue_if_the_condition_is_false()
        {
            // Arrange
            TestController.Test(
                "My test",
                () => TestController.SkipIf(false)
            );

            // Act & assert (no exception)
            foreach (var test in TestController.GetRegisteredTests())
                await test.ExecuteAsync();
        }
    }
}