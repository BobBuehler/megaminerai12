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

    public static BitArray GetPassable(bool walkInWater = false)
    {
        var impassible = new BitArray(Bb.Glaciers).Or(Bb.TheirSpawns).Or(Bb.TheirUnits).Or(Bb.OurUnits);
        if (!walkInWater)
        {
            impassible.Or(Bb.Water);
        }
        var spawning = Bb.OurSpawnSet.Union(Bb.OurPumpSet.Union(Bb.TheirPumpSet).SelectMany(pump => pump.GetPoints())).Where(p => Bb.tileLookup[p].IsSpawning);
        return spawning.ToBitArray().Or(impassible).Not();
    }

    public static LinkedList<Point> GetWalkingSteps(Point start, BitArray goals, bool walkInWater = false)
    {
        Point[] starts = { start };
        var passable = Solver.GetPassable(walkInWater);
        passable.Set(start, true);
        var route = Pather.AStar(starts, p => goals.Get(p), passable, (c, n) => 1, p => 0);
        if (route == null)
        {
            return null;
        }
        var steps = new LinkedList<Point>(route);
        steps.RemoveFirst();
        return steps;
    }

    public static void Move(Unit unit, BitArray goals)
    {
        Bb.ReadBoard();

        var stepCount = unit.MovementLeft;
        if (stepCount == 0)
        {
            return;
        }

        var steps = GetWalkingSteps(unit.ToPoint(), goals);
        foreach (Point p in steps)
        {
            if (stepCount == 0)
            {
                return;
            }
            unit.move(p.x, p.y);
            stepCount--;
        }
    }

    public static bool IsPumping(Pump pump)
    {
        var starts = pump.GetPoints();
        var goals = Bb.Glaciers;
        var passable = new BitArray(Bb.Water).Or(starts.ToBitArray()).Or(goals);

        return Pather.AStar(starts, p => goals.Get(p), passable, (c, n) => 1, p => 0) != null;
    }

    public static bool Attack(Unit attacker)
    {
        return Attack(attacker, Bb.TheirUnitsSet);
    }

    public static bool Attack(Unit attacker, IEnumerable<Unit> targets)
    {
        var target = targets.FirstOrDefault(t => t.HealthLeft > 0 && Manhattan(attacker.ToPoint(), t.ToPoint()) < attacker.Range);
        if (target != null)
        {
            attacker.attack(target);
            return true;
        }
        return false;
    }

    public static void MoveAndAttack(Unit attacker, IEnumerable<Unit> targets)
    {
        if (Attack(attacker, targets))
        {
            return;
        }

        if (attacker.MovementLeft == 0)
        {
            return;
        }

        var steps = GetWalkingSteps(attacker.ToPoint(), targets.Select(t => t.ToPoint()).ToBitArray());
        foreach (var step in steps)
        {
            attacker.move(step.x, step.y);
            if (Attack(attacker, targets) || attacker.MovementLeft == 0)
            {
                return;
            }
        }
    }
}
