using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Digger
{
    public static BitArray GetDiggableAndUnDug()
    {
        return new BitArray(Bb.OurPumps).Or(Bb.TheirPumps)
            .Or(Bb.OurSpawns).Or(Bb.TheirSpawns)
            .Or(Bb.OurUnits).Or(Bb.TheirUnits)
            .Or(Bb.Water).Or(Bb.Trenches)
            .Or(Bb.Glaciers).Not();
    }
    public static IEnumerable<Point> GetNeededTrenches(Pump pump)
    {
        var digworthy = GetDiggableAndUnDug();
        var starts = new HashSet<Point>(pump.GetPoints());
        var goals = Bb.GlaciersSet;
        var impassable = new BitArray(Bb.OurSpawns).Or(Bb.TheirSpawns).Or(Bb.OurPumps).Or(Bb.TheirPumps);
        var path = Pather.AStar(starts, p => goals.Contains(p), impassable.Not(), (c, n) => digworthy.Get(n) ? 1 : 0, p => 0);
        return path.Where(p => digworthy.Get(p));
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

    public static bool MoveAndDig(Unit digger, IEnumerable<Point> targets)
    {
        var digworhty = GetDiggableAndUnDug();
        if (Dig(digger, targets, digworhty))
        {
            return true;
        }

        if (digger.MovementLeft == 0)
        {
            return false;
        }

        var steps = Solver.GetWalkingSteps(digger.ToPoint(), targets.ToBitArray());
        if (steps == null)
        {
            return false;
        }
        var target = steps.Last.Value;
        steps.RemoveLast();
        foreach (var step in steps)
        {
            if (digger.MovementLeft > 0)
            {
                digger.move(step.x, step.y);
            }
            else
            {
                return false;
            }
        }
        digger.dig(Bb.tileLookup[target]);
        return true;
    }
}