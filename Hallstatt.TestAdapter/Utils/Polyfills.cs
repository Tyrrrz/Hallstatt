// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0
namespace System.Linq
{
    using Collections.Generic;

    internal static class PolyfillExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
            new(source, comparer);

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) =>
            source.ToHashSet(EqualityComparer<T>.Default);
    }
}
#endif