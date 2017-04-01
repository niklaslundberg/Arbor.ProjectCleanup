using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Arbor.ProjectCleanup
{
    public static class EnumerableExtensions
    {
        public static ImmutableArray<T> SafeToImmutableArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return ImmutableArray<T>.Empty;
            }

            if (enumerable is ImmutableArray<T>)
            {
                return (ImmutableArray<T>) enumerable;
            }

            return enumerable.ToImmutableArray();
        }

        public static T[] Add<T>(this T[] array, params T[] items)
        {
            List<T> list = array.ToList();

            list.AddRange(items);

            return list.ToArray();
        }
    }
}