using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

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


        // if I can tank
        //   Loop over our pumping pumps
        //     it it doesn't have a tank
        //       try to spawn a tank
        // while I can afford scouts
        //   spawn a scout

        var spawnable = Bb.GetOurSpawnable();
        var ourPumpingPumps = Bb.OurPumpSet.Where(p => p.station.SiegeAmount == 0 && Solver.WillBePumping(p));
        if (players[playerID()].Oxygen >= tankCost)
        {
            foreach (Pump pump in ourPumpingPumps)
            {
                var pumpPoints = new HashSet<Point>(pump.GetPoints());
                var tanksOnPump = Bb.OurTanksSet.Where(t => pumpPoints.Contains(t.ToPoint()));
                if (!tanksOnPump.Any())
                {
                    var openPoints = pumpPoints.Where(p => spawnable.Get(p));
                    if (openPoints.Any())
                    {
                        Bb.tileLookup[openPoints.First()].spawn((int)Types.Tank);
                    }
                }
                if (players[playerID()].Oxygen < tankCost)
                {
                    break;
                }
            }
        }

        var ourSpawnable = new HashSet<Point>(Bb.GetOurSpawnable().ToPoints());
        while ((players[playerID()].Oxygen >= scoutCost) && (ourSpawnable.Count != 0) && (Bb.TheirPumpSet.Count != 0))
        {
            var start = CalcSpawnPoint(ourSpawnable, Bb.TheirPumps.ToPoints());
            Bb.tileLookup[start].spawn((int)Types.Scout);
            ourSpawnable.Remove(start);
        }

        Bb.ReadBoard();


        var theirPumpingPumps = Bb.TheirPumpSet.Where(pump => Solver.WillBePumping(pump));
        var theirPumpingPumpsBits = theirPumpingPumps.SelectMany(p => p.GetPoints()).ToBitArray();
        var ourOwnedPumpingPumps = Bb.OurPumpSet.Where(p => Solver.WillBePumping(p));

        // Do Stuffs For Each Unit
        foreach (Unit u in Bb.OurUnitsSet)
        {
            if (u.Type == (int)Types.Scout)
            {
                if (theirPumpingPumps.Count() != 0)
                {
                    Solver.Move(u, theirPumpingPumpsBits);
                    Solver.Attack(u);
                }
                else if (Bb.TheirPumpSet.Count != 0)
                {
                    Solver.Move(u, Bb.TheirPumps);
                    Solver.Attack(u);
                }
                else
                {
                    Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
                }
            }
            else if (u.Type == (int)Types.Worker)
            {
                //Find nearest non pumping pump

            }
            else if (u.Type == (int)Types.Tank)
            {
                // Check whether our pump is pumping and move/die if it isn't
                if (!ourOwnedPumpingPumps.SelectMany(p => p.GetPoints()).Contains(u.ToPoint()))
                {
                    if (theirPumpingPumps.Count() != 0)
                    {
                        Solver.Move(u, theirPumpingPumpsBits);
                        Solver.Attack(u);
                    }
                    else
                    {
                        Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
                    }
                }
                else
                {
                    Solver.Attack(u);
                }
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
