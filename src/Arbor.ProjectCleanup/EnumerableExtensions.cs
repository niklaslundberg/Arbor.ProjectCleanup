using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Arbor.ProjectCleanup
{
    internal static class EnumerableExtensions
    {
        public static ImmutableArray<T> SafeToImmutableArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return ImmutableArray<T>.Empty;
            }

            if (enumerable is ImmutableArray<T>)
            {
                return (ImmutableArray<T>)enumerable;
            }

            return enumerable.ToImmutableArray();
        }

        public static T[] Add<T>(this T[] array, params T[] items)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<T> list = array.ToList();

            list.AddRange(items);

            return list.ToArray();
        }
    }
}
