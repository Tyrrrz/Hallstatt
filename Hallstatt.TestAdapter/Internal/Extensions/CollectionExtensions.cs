using System.Collections.Generic;

namespace Hallstatt.TestAdapter.Internal.Extensions
{
    internal static class CollectionExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
            new HashSet<T>(source, comparer);

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) =>
            source.ToHashSet(EqualityComparer<T>.Default);
    }
}