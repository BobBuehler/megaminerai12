using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Digger
{
    public IEnumerable<Point> GetNeededTrenches(Pump pump)
    {
        var starts = new HashSet<Point>(pump.GetPoints());
        var goals = Bb.GlaciersSet;
        var impassable = new BitArray(Bb.OurSpawns).Or(Bb.TheirSpawns);
        var path = Pather.AStar(starts, p => goals.Contains(p), impassable.Not(), (c, n) => Bb.Water.Get(n) ? 0 : 1, p => 0);
        return path.Where(p => !(Bb.Water.Get(p) || starts.Contains(p) || goals.Contains(p)));
    }
}