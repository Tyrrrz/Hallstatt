using System;
using System.Collections.Generic;

namespace Hallstatt
{
    /// <summary>
    /// Exposes contextual operations to configure test metadata.
    /// </summary>
    public class TestMetadataConfigurator
    {
        private readonly Dictionary<string, string?> _traits = new Dictionary<string, string?>(StringComparer.Ordinal);
        private bool _isSkipped;

        /// <summary>
        /// Adds a trait to the test.
        /// </summary>
        public TestMetadataConfigurator Trait(string name, string? value = null)
        {
            _traits[name] = value;
            return this;
        }

        /// <summary>
        /// Instructs the test to be skipped (conditionally).
        /// </summary>
        public TestMetadataConfigurator Skip(bool condition = true)
        {
            _isSkipped = condition;
            return this;
        }

        internal IReadOnlyDictionary<string, string?> GetTraits() => _traits;

        internal bool GetIsSkipped() => _isSkipped;
    }
}