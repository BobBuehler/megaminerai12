using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Digger
{
    public static IEnumerable<Point> GetNeededTrenches(Pump pump)
    {
        var starts = new HashSet<Point>(pump.GetPoints());
        var goals = Bb.GlaciersSet;
        var impassable = new BitArray(Bb.OurSpawns).Or(Bb.TheirSpawns);
        var path = Pather.AStar(starts, p => goals.Contains(p), impassable.Not(), (c, n) => Bb.Water.Get(n) || Bb.Trenches.Get(n) ? 0 : 1, p => 0);
        return path.Where(p => !(Bb.Water.Get(p) || starts.Contains(p) || goals.Contains(p)));
    }

    public static bool Dig(Unit digger, IEnumerable<Point> targets, BitArray diggable)
    {
        foreach (var n in Pather.GetNeighbors(digger.ToPoint(), diggable))
        {
            if (targets.Contains(n))
            {
                digger.dig(Bb.tileLookup[n]);
                return true;
            }
        }
        return false;
    }

    public static void MoveAndDig(Unit digger, IEnumerable<Point> targets)
    {
        var diggable = new BitArray(Bb.OurPumps).Or(Bb.TheirPumps).Or(Bb.Glaciers).Or(Bb.OurSpawns).Or(Bb.TheirSpawns).Not();
        if (Dig(digger, targets, diggable))
        {
            return;
        }

        if (digger.MovementLeft == 0)
        {
            return;
        }

        var steps = Solver.GetWalkingSteps(digger.ToPoint(), targets.ToBitArray());
        if (steps == null)
        {
            return;
        }
        var target = steps.Last.Value;
        steps.RemoveLast();
        foreach (var step in steps)
        {
            digger.move(step.x, step.y);
        }
        digger.dig(Bb.tileLookup[target]);
    }
}