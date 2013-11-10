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
        Console.WriteLine("Turn:{0}, P0:{1}, P1:{2}", turnNumber(), players[0].WaterStored, players[1].WaterStored);
        Bb.Init(this);
        int workerCost = 10;
        int scoutCost = 12;
        int tankCost = 15;

        Func<Point, Point, int> Manhattan = (a, b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        Func<IEnumerable<Point>, IEnumerable<Point>, Point> CalcSpawnPoint = (starts, goals) =>
        {
            var pairs = starts.SelectMany(s => goals.Select(g => new { start = s, goal = g }));
            return pairs.minByValue(p => Manhattan(p.start, p.goal)).start;
        };

        var pumpingPumpsSet = Bb.TheirPumpSet.Where(p => Solver.IsPumping(p)).SelectMany(p => p.GetPoints());
        var pumpingPumps = pumpingPumpsSet.ToBitArray();

        var notPumpingPumpsSet = Bb.OurPumpSet.Where(p => !Solver.IsPumping(p)).SelectMany(p => p.GetPoints());
        var notPumpingPumps = pumpingPumpsSet.ToBitArray();

        // Spawn Stuffs
        if (Bb.OurUnitsSet.Count < maxUnits())
        {
            var ourSpawnable = new HashSet<Point>(Bb.GetOurSpawnable().ToPoints());
            if (true)//(Bb.OurPumpSet.Count < 3 && Bb.OurScoutsSet.Count < 4)
            {
                while ((players[playerID()].Oxygen >= scoutCost) && (ourSpawnable.Count != 0) && (Bb.TheirPumpSet.Count != 0) && (Bb.OurScoutsSet.Count < 4))
                {
                    var start = CalcSpawnPoint(ourSpawnable, Bb.TheirPumps.ToPoints());
                    Bb.tileLookup[start].spawn((int)Types.Scout);
                    ourSpawnable.Remove(start);
                }
            }
            //if (Bb.OurPumpSet.Count == 0)
            //{
            //    while ((players[playerID()].Oxygen >= scoutCost) && (ourSpawnable.Count != 0) && (Bb.TheirPumpSet.Count != 0))
            //    {
            //        var start = CalcSpawnPoint(ourSpawnable, Bb.TheirPumps.ToPoints());
            //        Bb.tileLookup[start].spawn((int)Types.Scout);
            //        ourSpawnable.Remove(start);
            //    }
            //}
            var spawnableNotPumping = new HashSet<Point>(ourSpawnable.ToBitArray().And(notPumpingPumps).ToPoints());
            //if ((Bb.OurPumpSet.Count <= 3) && (notPumpingPumpsSet.Count() != 0))
            //{
            //    while ((players[playerID()].Oxygen >= workerCost) && (spawnableNotPumping.Count != 0) && (Bb.GlaciersSet.Count != 0))
            //    {
            //        var start = CalcSpawnPoint(spawnableNotPumping, Bb.Glaciers.ToPoints());
            //        Bb.tileLookup[start].spawn((int)Types.Worker);
            //        ourSpawnable.Remove(start);
            //        spawnableNotPumping.Remove(start);
            //    }
            //}
        }

        //Func<Point, IEnumerable<Pump>, Pump> CalcNearestPump = (start, goals) =>
        //{

            
        //    return goals.minByValue(g => Manhattan(start, g.GetPoints()));
        //};

        // Do Stuffs For Each Unit
        foreach (Unit u in Bb.OurUnitsSet)
        {
            // If you don't own the unit, ignore it.
            if (u.Owner != playerID())
                continue;
            if (u.Type == (int)Types.Scout)
            {
                if (pumpingPumpsSet.Count() != 0)
                {
                    Solver.Move(u, pumpingPumps);
                    Solver.Attack(u);
                }
                else if (Bb.TheirPumpSet.Count != 0)
                {
                    Solver.Move(u, Bb.TheirPumps);
                    Solver.Attack(u);
                }
                else
                    Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
            }
            else if (u.Type == (int)Types.Worker)
            {
                //Find nearest non pumping pump

            }
            else if (u.Type == (int)Types.Tank)
            {
            }
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
