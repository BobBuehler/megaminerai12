using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Solver
{
    public static int Manhattan(Point p1, Point p2)
    {
        return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
    }

    public static BitArray GetPassable()
    {
        var spawning = Bb.OurSpawnSet.Union(Bb.OurPumpSet.Union(Bb.TheirPumpSet).SelectMany(pump => pump.GetPoints())).Where(p => Bb.tileLookup[p].IsSpawning);
        return spawning.ToBitArray().Or(Bb.Water).Or(Bb.Glaciers).Or(Bb.TheirSpawns).Or(Bb.TheirUnits).Or(Bb.OurUnits).Not();
    }

    public static void Move(Unit unit, BitArray goals, bool nextTo = false)
    {
        Bb.ReadBoard();

        var steps = unit.MovementLeft;
        if (steps == 0)
        {
            return;
        }

        Point[] starts = { unit.ToPoint() };
        var passable = GetPassable();
        passable.Set(unit, true);
        var route = Pather.AStar(starts, p => goals.Get(p), passable, (c, n) => 1, p => 0);
        if (route == null)
        {
            return;
        }
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
        var starts = pump.GetPoints();
        var goals = Bb.Glaciers;
        var passable = new BitArray(Bb.Water).Or(starts.ToBitArray()).Or(goals);

        return Pather.AStar(starts, p => goals.Get(p), passable, (c, n) => 1, p => 0) != null;
    }

    public static void Attack(Unit attacker)
    {
        var target = Bb.TheirUnitsSet.FirstOrDefault(t => t.HealthLeft > 0 && Manhattan(attacker.ToPoint(), t.ToPoint()) < attacker.Range);
        if (target != null)
        {
            attacker.attack(target);
        }
    }

    public static void MoveAndAttack(Unit attacker, IEnumerable<Unit> targets)
    {
        var targetBb = targets.Select(t => t.ToPoint()).ToBitArray();
        var passable = GetPassable();
        passable.Or(targetBb);
        passable.Set(attacker, true);
        Move(attacker, targetBb);
    }
}
