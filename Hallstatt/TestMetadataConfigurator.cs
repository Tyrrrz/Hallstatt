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
        private bool _isParallel = true;
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
        /// Instructs whether the test can be parallelized.
        /// </summary>
        public TestMetadataConfigurator Parallel(bool isParallel = true)
        {
            _isParallel = isParallel;
            return this;
        }

        /// <summary>
        /// Instructs whether the test can be skipped.
        /// </summary>
        public TestMetadataConfigurator Skip(bool isSkipped = true)
        {
            _isSkipped = isSkipped;
            return this;
        }

        internal IReadOnlyDictionary<string, string?> GetTraits() => _traits;

        internal bool GetIsParallel() => _isParallel;

        internal bool GetIsSkipped() => _isSkipped;
    }
}