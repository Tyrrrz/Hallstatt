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
        /// Whether the test can be ran concurrently with others.
        /// </summary>
        public bool IsParallel { get; }

        /// <summary>
        /// Whether the test is skipped.
        /// </summary>
        public bool IsSkipped { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Test"/>.
        /// </summary>
        public Test(
            Guid id,
            string title,
            Assembly assembly,
            IReadOnlyDictionary<string, string?> traits,
            bool isParallel,
            bool isSkipped,
            Func<ValueTask> executeAsync)
        {
            Id = id;
            Title = title;
            Assembly = assembly;
            Traits = traits;
            IsParallel = isParallel;
            IsSkipped = isSkipped;
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