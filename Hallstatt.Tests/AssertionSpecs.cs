using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Hallstatt.Tests
{
    public class AssertionSpecs
    {
        [Fact]
        public void I_can_manually_report_assertion_failure()
        {
            Xunit.Assert.Throws<AssertionException>(() => Assert.Fail("Test"));
        }

        [Fact]
        public void I_can_assert_a_condition_and_get_no_error_if_it_is_true()
        {
            Assert.That(true);
        }

        [Fact]
        public void I_can_assert_a_condition_and_get_an_error_if_it_is_false()
        {
            Xunit.Assert.Throws<AssertionException>(
                () => Assert.That(false)
            );
        }

        [Fact]
        public void I_can_assert_an_exception_and_get_no_error_if_the_expected_exception_is_thrown()
        {
            Assert.Throws<IOException>(() => throw new IOException());
        }

        [Fact]
        public void I_can_assert_an_exception_and_get_an_error_if_no_exception_was_thrown()
        {
            Xunit.Assert.Throws<AssertionException>(
                () => Assert.Throws<IOException>(() => {})
            );
        }

        [Fact]
        public void I_can_assert_an_exception_and_get_an_error_if_another_was_thrown()
        {
            Xunit.Assert.Throws<AssertionException>(
                () => Assert.Throws<IOException>(() => throw new FileNotFoundException())
            );
        }

        [Fact]
        public void I_can_assert_a_derived_exception_and_get_no_error_if_derived_exception_was_thrown()
        {
            Assert.Throws<IOException>(() => throw new FileNotFoundException(), true);
        }

        [Fact]
        public async Task I_can_assert_an_async_exception_and_get_no_error_if_the_expected_exception_is_thrown()
        {
            await Assert.ThrowsAsync<IOException>(() => Task.FromException(new IOException()));
        }

        [Fact]
        public async Task I_can_assert_an_async_exception_and_get_an_error_if_no_exception_was_thrown()
        {
            await Xunit.Assert.ThrowsAsync<AssertionException>(
                () => Assert.ThrowsAsync<IOException>(() => Task.CompletedTask)
            );
        }

        [Fact]
        public async Task I_can_assert_an_async_exception_and_get_an_error_if_another_was_thrown()
        {
            await Xunit.Assert.ThrowsAsync<AssertionException>(
                () => Assert.ThrowsAsync<IOException>(() => Task.FromException(new FileNotFoundException()))
            );
        }

        [Fact]
        public async Task I_can_assert_a_derived_async_exception_and_get_no_error_if_derived_exception_was_thrown()
        {
            await Assert.ThrowsAsync<IOException>(() => Task.FromException(new FileNotFoundException()), true);
        }
    }
}