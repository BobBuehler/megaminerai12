using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
public class AI : BaseAI
{
    // Enum for types of units you can spawn.
    enum Types { Worker, Scout, Tank };

    public override string username()
    {
        return "Needs Review";
    }

    public override string password()
    {
        return "supersecret";
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        Bb.Init(this);

        Func<Point, Point, int> Manhattan = (a, b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        Func<IEnumerable<Point>, IEnumerable<Point>, Point> CalcSpawnPoint = (starts, goals) =>
        {
            var pairs = starts.SelectMany(s => goals.Select(g => new { start = s, goal = g }));
            return pairs.minByValue(p => Manhattan(p.start, p.goal)).start;
        };

        // Spawn Stuffs
        if (Bb.OurUnitsSet.Count < maxUnits())
        {
            while (players[playerID()].Oxygen >= 12)
            {
                var start = CalcSpawnPoint(Bb.OurSpawnSet, Bb.TheirPumpSet);
                tiles[Bb.GetOffset(start.x, start.y)].spawn((int)Types.Scout);
                Bb.OurSpawnSet.Remove(start);
            }
        }

        // Do Stuffs For Each Unit
        foreach (Unit i in Bb.OurUnitsSet)
        {
            // If you don't own the unit, ignore it.
            if (i.Owner != playerID())
                continue;
            Solver.Walk(i, Bb.TheirPumps);
        }
        return true;
    }

    /// <summary>
    /// This function is called once, before your first turn.
    /// </summary>
    public override void init() { }

    /// <summary>
    /// This function is called once, after your last turn.
    /// </summary>
    public override void end() { }

    public AI(IntPtr c) : base(c) { }
}
