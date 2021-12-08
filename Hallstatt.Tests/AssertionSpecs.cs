using System.IO;
using System.Threading.Tasks;
using Hallstatt.Assertions;
using Xunit;
using SutAssert = Hallstatt.Assertions.Assert;
using Assert = Xunit.Assert;

namespace Hallstatt.Tests;

public class AssertionSpecs
{
    [Fact]
    public void I_can_manually_report_assertion_failure()
    {
        Assert.Throws<AssertionException>(() => SutAssert.Fail("Test"));
    }

    [Fact]
    public void I_can_assert_a_condition_and_get_no_error_if_it_is_true()
    {
        SutAssert.That(true);
    }

    [Fact]
    public void I_can_assert_a_condition_and_get_an_error_if_it_is_false()
    {
        Assert.Throws<AssertionException>(
            () => SutAssert.That(false)
        );
    }

    [Fact]
    public void I_can_assert_an_exception_and_get_no_error_if_the_expected_exception_is_thrown()
    {
        SutAssert.Throws<IOException>(() => throw new IOException());
    }

    [Fact]
    public void I_can_assert_an_exception_and_get_an_error_if_no_exception_was_thrown()
    {
        Assert.Throws<AssertionException>(
            () => SutAssert.Throws<IOException>(() => {})
        );
    }

    [Fact]
    public void I_can_assert_an_exception_and_get_an_error_if_another_was_thrown()
    {
        Assert.Throws<AssertionException>(
            () => SutAssert.Throws<IOException>(() => throw new FileNotFoundException())
        );
    }

    [Fact]
    public void I_can_assert_a_derived_exception_and_get_no_error_if_derived_exception_was_thrown()
    {
        SutAssert.Throws<IOException>(() => throw new FileNotFoundException(), true);
    }

    [Fact]
    public async Task I_can_assert_an_async_exception_and_get_no_error_if_the_expected_exception_is_thrown()
    {
        await SutAssert.ThrowsAsync<IOException>(() => Task.FromException(new IOException()));
    }

    [Fact]
    public async Task I_can_assert_an_async_exception_and_get_an_error_if_no_exception_was_thrown()
    {
        await Assert.ThrowsAsync<AssertionException>(
            () => SutAssert.ThrowsAsync<IOException>(() => Task.CompletedTask)
        );
    }

    [Fact]
    public async Task I_can_assert_an_async_exception_and_get_an_error_if_another_was_thrown()
    {
        await Assert.ThrowsAsync<AssertionException>(
            () => SutAssert.ThrowsAsync<IOException>(() => Task.FromException(new FileNotFoundException()))
        );
    }

    [Fact]
    public async Task I_can_assert_a_derived_async_exception_and_get_no_error_if_derived_exception_was_thrown()
    {
        await SutAssert.ThrowsAsync<IOException>(() => Task.FromException(new FileNotFoundException()), true);
    }
}