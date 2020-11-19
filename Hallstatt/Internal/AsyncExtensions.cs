using System;
using System.Threading.Tasks;

namespace Hallstatt.Internal
{
    internal static class AsyncExtensions
    {
        public static Func<ValueTask> ToValueTaskFunc(this Action action) => () =>
        {
            action();
            return default;
        };

        public static Func<T1, ValueTask> ToValueTaskFunc<T1>(this Action<T1> action) => p =>
        {
            action(p);
            return default;
        };
    }
}