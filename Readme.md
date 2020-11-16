# Hallstatt

[![Build](https://github.com/Tyrrrz/Hallstatt/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/Hallstatt/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/Hallstatt/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/Hallstatt)
[![Version](https://img.shields.io/nuget/v/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Downloads](https://img.shields.io/nuget/dt/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

**Project status: active**.

Hallstatt is a testing framework for C# that is specifically designed to be as simple and straightforward as possible, drawing on inspirations from [Expecto](https://github.com/haf/expecto).
Instead of the traditional approach that involves writing tests as class methods found in xUnit, NUnit, MSTest, Hallstatt tests are defined directly in code, bypassing all of the limitations imposed by methods and attributes.

## Download

- [NuGet](https://nuget.org/packages/Hallstatt): `dotnet add package Hallstatt`

## Features

TODO

## Usage

### Creating a test project

It's very easy to convert an existing test project or create a new one. All you need to do is make sure your test `.csproj` file looks like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    
    <!-- This disables the entrypoint generated by test SDK -->
    <GenerateProgramFile>false</GenerateProgramFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="16.8.0" />
    <PackageReference Include="Hallstatt" Version="0.1.0" /> <!-- (update to latest) -->
    <PackageReference Include="Hallstatt.TestAdapter" Version="0.1.0" /> <!-- (update to latest) -->

    <!-- Make sure there are no references to xUnit, NUnit, MSTest, etc -->
    <!-- Any other package references are okay -->
  </ItemGroup>

</Project>
```

### Basic example

Once you're set up, create a new test file:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4", () =>
{
    var result = 2 + 2;
    Assert.That(result).IsEqualTo(4);
});
```

Note that here we are relying on the [top-level statements](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/exploration/top-level-statements) feature introduced in C# 9.
This is how Hallstatt was designed to be used, but you can also define an explicit `Main()` method instead.

If we run `dotnet test --list-tests` to see the list of available tests, we should see the following:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 2 and 2 equals 4
```

Of course, we can also run `dotnet test` to execute our test:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 58 ms
```

### Assertions

Hallstatt comes with a basic, but extensible assertion module represented by the `Assert` static class:

```csharp
Test("Assertions should work", () =>
{
    Assert.That(true).IsTrue();
    Assert.That(42).IsGreaterThan(13);
    Assert.That("foo").IsNotNullOrWhiteSpace();
    Assert.That(() => DoSomething()).Throws<InvalidOperationException>();
});
```

You can also easily extend it with your own assertions by creating extension methods on `Assertions<T>`:

```csharp
public static class MyAssertionExtensions
{
    // Asserts that a file or directory exists
    public static void Exists(this Assertions<FileSystemInfo> assertions)
    {
        if (!assertions.Source.Exists)
            Assert.Fail($"Expected path '{assertions.Source}' to exist.");
    }
}
```

The built-in assertion module is meant to provide a foundation to get you started, but might be insufficient for more complicated use-cases.
For a more feature-complete assertion library, it's recommended to install [FluentAssertions](https://github.com/fluentassertions/fluentassertions) or [Shouldly](https://github.com/shouldly/shouldly) and use them in your Hallstatt tests.

### Parametrized tests

Parametrized tests can be created by calling `TestMany()`:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

TestMany(
    // Test parameters
    new[]
    {
        new {Left = 1, Right = 3, Result = 4},
        new {Left = 5, Right = 2, Result = 7},
        new {Left = 1, Right = -2, Result = -1}
    },

    // Test title
    p => $"Sum of {p.Left} and {p.Right} equals {p.Result}",

    // Test body
    p =>
    {
        var result = p.Left + p.Right;
        Assert.That(result).IsEqualTo(p.Result);
    }
);
```

The above code registers the following 3 tests:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 1 and 3 equals 4
    Sum of 5 and 2 equals 7
    Sum of 1 and -2 equals -1
```

### Skipping tests

Tests can be skipped at any point by calling `Skip()` inside of test body:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

Test("Skipped test", () =>
{
    Skip("Not implemented yet");
    
    // Not going to be executed
    Assert.That(true).IsFalse();
});

Test("Conditionally skipped test", () =>
{
    // Skip when not running on Windows
    SkipIf(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    
    var registry = Registry.CurrentUser.OpenSubKey("foo");
    Assert.That(registry.GetValue()).IsNotNull();
});
```

### Assigning traits

Tests can have arbitrary traits (key-value pairs) assigned to them:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4",
    o =>
    {
        o.Trait("Category", "MySpecialTests");
        o.Trait("Foo", "Bar");
    }
    () =>
    {
        var result = 2 + 2;
        Assert.That(result).IsEqualTo(4);
    }
);
```

You can use traits, among other things, to [filter tests](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test#filter-option-details):

```txt
dotnet test --filter Category=MySpecialTests
```
