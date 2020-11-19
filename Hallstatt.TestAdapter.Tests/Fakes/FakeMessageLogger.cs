using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Hallstatt.TestAdapter.Tests.Fakes
{
    public partial class FakeMessageLogger : IMessageLogger
    {
        private readonly List<MessageEntry> _messages = new List<MessageEntry>();

        public void SendMessage(TestMessageLevel testMessageLevel, string message) =>
            _messages.Add(new MessageEntry(testMessageLevel, message));

        public IReadOnlyList<MessageEntry> GetMessages() => _messages.ToArray();
    }

    public partial class FakeMessageLogger
    {
        public class MessageEntry
        {
            public TestMessageLevel Level { get; }

            public string Message { get; }

            public MessageEntry(TestMessageLevel level, string message)
            {
                Level = level;
                Message = message;
            }
        }
    }
}