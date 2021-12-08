using System;

namespace Hallstatt.Assertions;

/// <summary>
/// Thrown in case of a failed assertion.
/// </summary>
public class AssertionException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="AssertionException"/>.
    /// </summary>
    public AssertionException(string message = "")
        : base(message) {}
}