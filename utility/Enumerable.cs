using System.Collections.Generic;
using System.Linq;
using System.Threading;

public static class EnumerableUtil
{
    static IEnumerator<T> NextEnumerator<T>(Queue<IEnumerator<T>> queue, IEnumerator<T> e)
    {
        if (e == null)
        {
            if (e.MoveNext())
            {
                queue.Enqueue(e);
            }
            else
            {
                return null;
            }
        }
        return e;
    }

    public static IEnumerable<T> Weave<T>(this IEnumerable<IEnumerable<T>> source)
    {
        LinkedList<IEnumerator<T>> queue = null;
        foreach (var s in source)
        {
            var e1 = s.GetEnumerator();
            if (e1.MoveNext())
            {
                yield return e1.Current;
                LazyInitializer.EnsureInitialized(ref queue);
                queue.AddLast(e1);
            }
            else
            {
                e1.Dispose();
            }
        }

        if (queue != null)
        {

            // Start by getting the first enuerator if it exists.
            var n = queue.First;
            while (n != null)
            {
                while (n != null)
                {
                    // Loop through all the enumerators..
                    var e2 = n.Value;
                    if (e2.MoveNext())
                    {
                        yield return e2.Current;
                        n = n.Next;
                    }
                    else
                    {
                        // None left? Remove the node.
                        var r = n;
                        n = n.Next;
                        queue.Remove(r);
                        e2.Dispose();
                    }
                }
                // Reset and try again.
                n = queue.First;
            }
        }


    }

    public static LazyList<T> Memoize<T>(this IEnumerable<T> list)
    {
        return new LazyList<T>(list);
    }

    public static IEnumerable<T> OfType<TSource, T>(this IEnumerable<TSource> list)
    {
        return list.Where(e => e is T).Cast<T>();
    }

    public static void AddThese<T>(this IList<T> target, IEnumerable<T> values)
    {
        foreach (var v in values)
        {
            target.Add(v);
        }
    }
}