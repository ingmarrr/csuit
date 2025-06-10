using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils;

public static class EnumerableExtensions
{
    public static IEnumerable<int> ToInc(this int from, int to)
    {
        return To(from, to + 1);
    }

    /// `To` is the *exclusive* upper bound
    public static IEnumerable<int> To(this int from, int to)
    {
        if (from < to)
        {
            while (from < to)
            {
                yield return from++;
            }
        }
        else
        {
            while (from > to)
            {
                yield return from--;
            }
        }
    }
    
    public static IEnumerable<T> Step<T>(this IEnumerable<T> source, int step)
    {
        if (step == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "Param cannot be zero.");
        }

        return source.Where((x, i) => i % step == 0);
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> sequence)
    {
        return sequence.Where(e => e != null)!;
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> sequence)
        where T : struct
    {
        return sequence.Where(e => e != null).Select(e => e!.Value);
    }

    public static bool None<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
    {
        return !sequence.Any(predicate);
    }
}


