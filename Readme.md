# Hallstatt

[![Build](https://github.com/Tyrrrz/Hallstatt/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/Hallstatt/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/Hallstatt/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/Hallstatt)
[![Version](https://img.shields.io/nuget/v/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Downloads](https://img.shields.io/nuget/dt/Hallstatt.svg)](https://nuget.org/packages/Hallstatt)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

**Project status: active**.

Hallstatt is a simple and straightforward testing framework for C#. Instead of relying on the traditional approach for defining tests through class methods and attributes, Hallstatt tests are defined directly in code using lambdas, avoiding many associated limitations in the process. It's largely inspired by JavaScript testing frameworks and F#'s [Expecto](https://github.com/haf/expecto).

## Download

- [NuGet](https://nuget.org/packages/Hallstatt): `dotnet add package Hallstatt`

## Features

- Does not rely on attributes and reflection for defining tests
- Allows any string literal to be used as a test name
- Statically-typed test parametrization without boilerplate
- Succinct and expressive interface
- Targets .NET Standard 2.0+

## Usage

### Creating a test project

To use Hallstatt follow the next steps:

1. Create a new or open existing an .NET project and change the `.csproj` file so that it looks similar to this: 

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    <GenerateProgramFile>false</GenerateProgramFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="16.8.0" />
  </ItemGroup>

</Project>
```

2. Install latest version of `Hallstatt` from NuGet: `dotnet add package Hallstatt`

3. Install latest version of `Hallstatt.TestAdapter` from NuGet: `dotnet add package Hallstatt.TestAdapter`

4. Define your tests in code

Note, if you're migrating from another test framework, make sure to remove all of its associated packages to avoid conflicts.

### Basic example

Once the project is set up, you can write a test like so:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4", () =>
{
    var result = 2 + 2;
    Assert.That(result == 4);
});
```

By doing the above, we've defined a test called `Sum of 2 and 2 equals 4`, which executes a simple mathematical operation and validates the result. Note that calling `Test(...)` does not actually run the test, but just registers it for the test adapter.

The snippet above also uses the [top-level statements](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/exploration/top-level-statements) feature introduced in C# 9. Functionally, it's identical to this:

```csharp
using Hallstatt;
using static Hallstatt.TestController;

public static class MyTests
{
    public static void Main()
    {
        Test("Sum of 2 and 2 equals 4", () =>
        {
            var result = 2 + 2;
            Assert.That(result == 4);
        });
    }
}
```

Now, if we run `dotnet test --list-tests` on our test project, we should see it print the name of the test that we have defined:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 2 and 2 equals 4
```

We can also run `dotnet test` to execute it:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 58 ms
```

### Assertions

Hallstatt comes with a basic assertion module represented by the `Assert` class:

```csharp
Test("My test", () =>
{
    Assert.That(1 == 2);
    Assert.Throws<InvalidOperationException>(() => DoSomething());
    Assert.Fail("oops...");
});
```

These assertion utilities should be enough to get you started, but they are intentionally very simple and unambitious. It is **strongly recommended to use an external feature-complete library** like [FluentAssertions](https://github.com/fluentassertions/fluentassertions) or [Shouldly](https://github.com/shouldly/shouldly) to perform assertions in your Hallstatt tests.

### Parametrized tests

The concept of "parametrized tests" dwindles when we don't have to concern ourselves with the inherent limitations of methods for defining tests. After all, we can simply do this:

```csharp
var parameters = new[]
{
    // Anonymous objects
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

The above approach is perfectly valid, but Hallstatt also provides an utility that allows you to do the same thing more directly:

```csharp
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

Essentially, we've registered 3 tests that validate the same code against different inputs. Running test discovery should show us the following:

```txt
Microsoft (R) Test Execution Command Line Tool Version 16.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

The following Tests are available:
    Sum of 1 and 3 equals 4
    Sum of 5 and 2 equals 7
    Sum of 1 and -2 equals -1
```

### Skipping tests

Tests can be skipped at any point by calling `Skip()` or `SkipIf(...)`:

```csharp
Test("Skipped test", () =>
{
    Skip("Not implemented yet");
    
    // Not going to be executed
    Assert.That(false);
});

Test("Conditionally skipped test", () =>
{
    // Skip when not running on Windows
    SkipIf(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    
    var registry = Registry.CurrentUser.OpenSubKey("foo");
    Assert.That(registry.GetValue() != null);
});
```

Internally, `Skip()` and `SkipIf(...)` throw `TestSkippedException` which is special cased by the test adapter. This exception causes the test to be reported as skipped instead of failed when it is thrown.

### Assigning traits

Just like in other frameworks, tests in Hallstatt can have arbitrary traits (key-value pairs) assigned to them:

```csharp
Test("Sum of 2 and 2 equals 4",
    o =>
    {
        o.Trait("Category", "MySpecialTests");
        o.Trait("Foo");
    }
    () =>
    {
        var result = 2 + 2;
        Assert.That(result).IsEqualTo(4);
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
        Assert.That(result).IsEqualTo(p.Result);
    }
);
```

Traits can be used, among other things, for [filtered test runs](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test#filter-option-details):

```txt
dotnet test --filter Category=MySpecialTests
```

### Comparison with the traditional approach

To understand the advantages of using Hallstatt, let's consider how the same test can be implemented using different frameworks. For example, here is a simple parametrized test written in xUnit:

```csharp
public static TheoryData<string?, GeoLocation?> GeoLocationParseTestCases =>
    new TheoryData<string?, GeoLocation?>
    {
        // Valid

        {"41.25 -120.9762", new GeoLocation(41.25, -120.9762)},
        {"41.25, -120.9762", new GeoLocation(41.25, -120.9762)},
        {"41.25,-120.9762", new GeoLocation(41.25, -120.9762)},
        {"-41.25, -120.9762", new GeoLocation(-41.25, -120.9762)},
        {"41.25, 120.9762", new GeoLocation(41.25, 120.9762)},
        {"41.25°N, 120.9762°W", new GeoLocation(41.25, -120.9762)},
        {"41.25N 120.9762W", new GeoLocation(41.25, -120.9762)},
        {"41.25N, 120.9762W", new GeoLocation(41.25, -120.9762)},
        {"41.25 N, 120.9762 W", new GeoLocation(41.25, -120.9762)},
        {"41.25 S, 120.9762 W", new GeoLocation(-41.25, -120.9762)},
        {"41.25 S, 120.9762 E", new GeoLocation(-41.25, 120.9762)},
        {"41, 120", new GeoLocation(41, 120)},
        {"-41, -120", new GeoLocation(-41, -120)},
        {"41 N, 120 E", new GeoLocation(41, 120)},
        {"41 N, 120 W", new GeoLocation(41, -120)},

        // Invalid

        {"41.25; -120.9762", null},
        {"-41.25 S, 120.9762 E", null},
        {"41.25", null},
        {"", null},
        {null, null}
    };

[Theory]
[MemberData(nameof(GeoLocationParseTestCases))]
public void I_can_type_in_my_location_manually_using_standard_notation(
    string? str, GeoLocation? expectedResult)
{
    // Act & assert
    GeoLocation.TryParse(str).Should().Be(expectedResult);
}
``` 

And here is the same test defined in Hallstatt:

```csharp
TestMany(
    new[]
    {
        // Valid
        
        new {Input = "41.25 -120.9762", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25, -120.9762", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25,-120.9762", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "-41.25, -120.9762", Output = new GeoLocation(-41.25, -120.9762)},
        new {Input = "41.25, 120.9762", Output = new GeoLocation(41.25, 120.9762)},
        new {Input = "41.25°N, 120.9762°W", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25N 120.9762W", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25N, 120.9762W", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25 N, 120.9762 W", Output = new GeoLocation(41.25, -120.9762)},
        new {Input = "41.25 S, 120.9762 W", Output = new GeoLocation(-41.25, -120.9762)},
        new {Input = "41.25 S, 120.9762 E", Output = new GeoLocation(-41.25, 120.9762)},
        new {Input = "41, 120", Output = new GeoLocation(41, 120)},
        new {Input = "-41, -120", Output = new GeoLocation(-41, -120)},
        new {Input = "41 N, 120 E", Output = new GeoLocation(41, 120)},
        new {Input = "41 N, 120 W", Output = new GeoLocation(41, -120)},

        // Invalid

        new {Input = "41.25; -120.9762", Output = default(GeoLocation?)},
        new {Input = "-41.25 S, 120.9762 E", Output = default(GeoLocation?)},
        new {Input = "41.25", Output = default(GeoLocation?)},
        new {Input = "", Output = default(GeoLocation?)},
        new {Input = default(string), Output = default(GeoLocation?)}
    },

    p => $"I can type in my location manually using standard notation ({p})",

    p =>
    {
        GeoLocation.TryParse(p.Input).Should().Be(p.Output);
    }
);
```

As you can see, the second approach has the advantage of being more localized, type-safe (without the need for analyzers), and requires less code. Additionally, by not relying on methods, Hallstatt tests can use normal human-readable strings in test names without having to rely on special transformations.
