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
        var path = Pather.AStar(starts, p => goals.Contains(p), impassable.Not(), (c, n) => Bb.Water.Get(n) ? 0 : 1, p => 0);
        return path.Where(p => !(Bb.Water.Get(p) || starts.Contains(p) || goals.Contains(p)));
    }

    //public static void Dig(Unit digger, IEnumerable<Point> targets)
    //{
    //    Pather.GetNeighbors(digger.ToPoint(), 
    //}

    //public static void MoveAndDig(Unit digger, IEnumerable<Point> targets)
    //{
    //    var diggable = 
    //    Pather.GetNeighbors(
    //    if (Attack(attacker, targets))
    //    {
    //        return;
    //    }

    //    if (attacker.MovementLeft == 0)
    //    {
    //        return;
    //    }

    //    var steps = GetWalkingSteps(attacker.ToPoint(), targets.Select(t => t.ToPoint()).ToBitArray());
    //    foreach (var step in steps)
    //    {
    //        attacker.move(step.x, step.y);
    //        if (Attack(attacker, targets) || attacker.MovementLeft == 0)
    //        {
    //            return;
    //        }
    //    }
    //}
}