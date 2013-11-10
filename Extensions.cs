using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Extensions
{
    public static T minByValue<T, V>(this IEnumerable<T> source, Func<T, V> selector)
    {
        var comparer = Comparer<V>.Default;
        T first = source.First();
        T min = first;
        V minValue = selector(first);
        foreach (T s in source)
        {
            V value = selector(s);
            if (comparer.Compare(value, minValue) < 0)
            {
                min = s;
                minValue = value;
            }
        }
        return min;
    }

    public static bool Get(this BitArray b, Point p)
    {
        return b.Get(Bb.GetOffset(p.x, p.y));
    }

    public static void Set(this BitArray b, int x, int y, bool value)
    {
        b.Set(Bb.GetOffset(x, y), value);
    }

    public static void Set(this BitArray b, Mappable m, bool value)
    {
        b.Set(m.X, m.Y, value);
    }

    public static void Set(this BitArray b, Point p, bool value)
    {
        b.Set(p.x, p.y, value);
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var s in source)
        {
            action(s);
        }
    }

    public static Point ToPoint(this Mappable mappable)
    {
        return new Point(mappable.X, mappable.Y);
    }

    public static BitArray ToBitArray(this IEnumerable<Point> points)
    {
        var bits = new BitArray(Bb.size);
        points.ForEach(p => bits.Set(p, true));
        return bits;
    }
}
