using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Solver
{
    public static void Walk(Unit unit, BitArray goals)
    {
        var steps = unit.MovementLeft;
        if (steps == 0)
        {
            return;
        }

        Point[] starts = { new Point(unit.X, unit.Y) };
        var passable = new BitArray(Bb.Water).Or(Bb.Glaciers).Or(Bb.TheirSpawns).Or(Bb.TheirUnits).Or(Bb.OurUnits).Not();
        passable.Set(Bb.GetOffset(unit.X, unit.Y), false);
        var route = Pather.AStar(starts, p => goals.Get(p), passable, (c, n) => 1, p => 0);
        bool first = true;
        foreach (Point p in route)
        {
            if (steps == 0)
            {
                return;
            }
            if (first)
            {
                first = false;
                continue;
            }
            unit.move(p.x, p.y);
            steps--;
        }
    }

    public static bool IsPumping(Pump pump)
    {
        return false;
    }
}
