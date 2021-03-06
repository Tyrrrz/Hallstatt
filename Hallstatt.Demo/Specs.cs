﻿using System.Threading.Tasks;
using Hallstatt.Assertions;
using static Hallstatt.TestController;

Test("Sum of 2 and 2 equals 4", () =>
{
    var result = 2 + 2;
    Assert.That(result == 4);
});

Test("Sum of 2 and 2 equals 4 (async)", async () =>
{
    var result = await Task.Run(() => 2 + 2);
    Assert.That(result == 4);
});

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
        Assert.That(result == p.Result);
    }
);

Test("Skipped test", o => o.Skip(), () =>
{
    // Never ran
    Assert.That(false);
});

Test("Test with traits",
    o =>
    {
        o.Trait("Category", "SpecialTests");
    },
    () =>
    {
        Assert.That(true);
    }
);