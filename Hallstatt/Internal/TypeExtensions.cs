using System;

namespace Hallstatt.Internal
{
    internal static class TypeExtensions
    {
        public static bool IsAssignableTo(this Type type, Type targetType) =>
            targetType.IsAssignableFrom(type);
    }
}