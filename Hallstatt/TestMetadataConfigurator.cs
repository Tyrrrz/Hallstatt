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

        /// <summary>
        /// Adds a trait to the test.
        /// </summary>
        public TestMetadataConfigurator Trait(string name, string? value = null)
        {
            _traits[name] = value;
            return this;
        }

        internal IReadOnlyDictionary<string, string?> GetTraits() => _traits;
    }
}