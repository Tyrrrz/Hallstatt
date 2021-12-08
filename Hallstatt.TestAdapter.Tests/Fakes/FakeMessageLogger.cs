using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Hallstatt.TestAdapter.Tests.Fakes;

public record FakeMessageEntry(
    TestMessageLevel Level,
    string Message
);

public class FakeMessageLogger : IMessageLogger
{
    private readonly ConcurrentBag<FakeMessageEntry> _messages = new();

    public void SendMessage(TestMessageLevel testMessageLevel, string message) =>
        _messages.Add(new FakeMessageEntry(testMessageLevel, message));

    public IReadOnlyList<FakeMessageEntry> GetMessages() => _messages.ToArray();
}