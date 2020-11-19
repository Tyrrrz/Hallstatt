using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace Hallstatt
{
    /// <summary>
    /// Test registration.
    /// </summary>
    public class Test
    {
        private readonly Func<ValueTask> _executeAsync;

        /// <summary>
        /// Test ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Title of the test.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Assembly which registered the test.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Associated tags.
        /// </summary>
        public IReadOnlyDictionary<string, string?> Traits { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Test"/>.
        /// </summary>
        public Test(
            Guid id,
            string title,
            Assembly assembly,
            IReadOnlyDictionary<string, string?> traits,
            Func<ValueTask> executeAsync)
        {
            Id = id;
            Title = title;
            Assembly = assembly;
            Traits = traits;
            _executeAsync = executeAsync;
        }

        /// <summary>
        /// Executes the test.
        /// </summary>
        public ValueTask ExecuteAsync() => _executeAsync();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Title;
    }
}