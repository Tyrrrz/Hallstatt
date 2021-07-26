# Hallstatt

[![Build](https://github.com/Tyrrrz/Hallstatt/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/Hallstatt/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/Hallstatt/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/Hallstatt)
[![Version](https://img.shields.io/nuget/v/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Downloads](https://img.shields.io/nuget/dt/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

⚠️ **Project status: suspended**. Waiting on a [C# language proposal](https://github.com/dotnet/csharplang/issues/4163).

Hallstatt is a simple and straightforward testing framework for C#.
Instead of relying on the traditional approach for defining tests through class methods and attributes, Hallstatt tests are defined using top-level statements and lambdas, resulting in more concise code and avoiding many associated limitations in the process.

This library is inspired by JavaScript testing frameworks and F#'s [Expecto](https://github.com/haf/expecto).

**Note: this is an experimental project and is not yet recommended for production use**.

## Download

📦 [NuGet (Hallstatt)](https://nuget.org/packages/Hallstatt): `dotnet add package Hallstatt`

📦 [NuGet (Hallstatt.TestAdapter)](https://nuget.org/packages/Hallstatt.TestAdapter): `dotnet add package Hallstatt.TestAdapter`

## Usage

### Getting started

To use Hallstatt, take the following steps:

1. Install `Hallstatt` package in your test project
2. Install `Hallstatt.TestAdapter` package in your test project
3. Install `Microsoft.NET.Test.Sdk` package in your test project (or update to latest)
4. Add `<GenerateProgramFile>false</GenerateProgramFile>` to your test project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- ... -->
    <!-- Add the following line: -->
    <GenerateProgramFile>false</GenerateProgramFile>    
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="x.x.x" />
    <PackageReference Include="Hallstatt" Version="x.x.x" />
    <PackageReference Include="Hallstatt.TestAdapter" Version="x.x.x" />
    <!-- ... -->
  </ItemGroup>

</Project>
```

4. Start writing tests!

### Basic example

In order to define a test, simply add a top-level call to `TestController.Test(...)` specifying the name of the test and the lambda expression used to evaluate it:

```csharp
using Hallstatt;
using Hallstatt.Assertions;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4", () =>
{
    var result = 2 + 2;
    Assert.That(result == 4);
});
```

To execute the test, run `dotnet test`:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 58 ms
```

To get a list of tests defined in a project, run `dotnet test --list-tests`:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 2 and 2 equals 4
```

### Assertions

Hallstatt comes with a rudimentary assertion module represented by the `Assert` class, which can be used to verify simple claims:

```csharp
using Hallstatt;
using Hallstatt.Assertions;
using static Hallstatt.TestController;

Test("My test", () =>
{
    Assert.That(1 == 2);
    Assert.Throws<InvalidOperationException>(() => DoSomething());
    Assert.Fail("oops...");
});
```

These utilities should be enough to get started, but they are intentionally limited and unambitious.
It is **strongly recommended to use an external feature-complete assertion library** like [FluentAssertions](https://github.com/fluentassertions/fluentassertions) or [Shouldly](https://github.com/shouldly/shouldly) in your Hallstatt tests.

Plugging an external assertion library does not require any configuration:

```csharp
using Hallstatt;
using FluentAssertions;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4", () =>
{
    var result = 2 + 2;
    result.Should().Be(4);
});
```

### Parametrized tests

One of the main benefits of defining tests dynamically is the ability to compose them easily, as you would with normal functions.
As an example, here's how you define a so-called parametrized test in Hallstatt:

```csharp
using Hallstatt;
using Hallstatt.Assertions;
using static Hallstatt.TestController;

TestMany(
    // Test parameters
    new[]
    {
        // Anonymous objects allow us to author test cases quickly,
        // while stil maintaining complete type safety!
        
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
        Assert.That(result == p.Result);
    }
);
```

Under the hood, `TestMany(...)` is just a helpful utility that takes a list of test cases and registers a corresponding test for each of them.
Essentially, the above code is functionally equivalent to the following (slightly less eloquent) snippet:

```csharp
using Hallstatt;
using Hallstatt.Assertions;
using static Hallstatt.TestController;

var parameters = new[]
{
    new {Left = 1, Right = 3, Result = 4},
    new {Left = 5, Right = 2, Result = 7},
    new {Left = 1, Right = -2, Result = -1}
}

foreach (var parameter in parameters)
{
    Test($"Sum of {parameter.Left} and {parameter.Right} equals {parameter.Result}", () =>
    {
        var result = parameter.Left + parameter.Right;
        Assert.That(result == parameter.Result);
    });
}
```

Using `TestMany(...)` as shown earlier will result in the following tests being registered:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 1 and 3 equals 4
    Sum of 5 and 2 equals 7
    Sum of 1 and -2 equals -1
```

### Test configuration

Both `Test(...)` and `TestMany(...)` have an overload that take an additional lambda expression that can be used to configure various properties.
For example, you can assign traits to tests, which can be useful for filtered runs or other integrations:

```csharp
Test("Sum of 2 and 2 equals 4",
    o =>
    {
        // Key & value
        o.Trait("Category", "MySpecialTests");
        
        // Key & no value
        o.Trait("Foo");
    }
    () =>
    {
        var result = 2 + 2;
        Assert.That(result == 4);
    }
);

TestMany(
    new[]
    {
        new {Left = 1, Right = 3, Result = 4},
        new {Left = 5, Right = 2, Result = 7},
        new {Left = 1, Right = -2, Result = -1}
    },
    (p, o) =>
    {
        o.Trait("Category", "MySpecialTests");
        o.Trait("Foo");
        o.Trait("Parametrized", p.Left.ToString());
    }
    p => $"Sum of {p.Left} and {p.Right} equals {p.Result}",
    p =>
    {
        var result = p.Left + p.Right;
        Assert.That(result == p.Result);
    }
);
```

Besides that, you can also mark tests as skipped to (temporarily) exclude them from the suite:

```csharp
Test("Skipped test", o => o.Skip(), () =>
{
    // Not going to be executed
    Assert.That(false);
});

Test("Conditionally skipped test",

    // Skip when not running on Windows
    o => o.Skip(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)),

    () =>
    {
        var registry = Registry.CurrentUser.OpenSubKey("foo");
        Assert.That(registry.GetValue() is not null);
    }
);
```
