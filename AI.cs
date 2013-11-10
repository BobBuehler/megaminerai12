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
    Pump finalPump = null;
    int finalStageCounter = 0;
    int stage = 1;

    public override string username()
    {
        return "Needs Review";
    }

    public override string password()
    {
        return "supersecret";
    }

    public Dictionary<int, Point> tankReturn = new Dictionary<int, Point>();

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
            if (!starts.Any() || !goals.Any())
            {
                return new Point(-1, -1);
            }

            var closestPath = Pather.AStar(starts, p => goals.Contains(p), Solver.GetPassable().Or(goals.ToBitArray()), (c, n) => 1, p => 0);
            if (closestPath != null)
            {
                return closestPath.First();
            }

            var pairs = starts.SelectMany(s => goals.Select(g => new { start = s, goal = g }));
            return pairs.minByValue(p => Manhattan(p.start, p.goal)).start;
        };


        if (finalStageCounter < 40)
        {

            // if I can tank
            //   Loop over our pumping pumps
            //     it it doesn't have a tank
            //       try to spawn a tank
            // while I can afford scouts
            //   spawn a scout

            //bool shouldSpwanScouts = true;
            //var spawnable = Bb.GetOurSpawnable();
            //var ourPumpingPumps = Bb.OurPumpSet.Where(p => p.station.SiegeAmount == 0 && Solver.WillBePumping(p));
            //if (Bb.TheirWorkersSet.Count == 0)
            //{
            //    foreach (Pump pump in ourPumpingPumps)
            //    {
            //        var pumpPoints = new HashSet<Point>(pump.GetPoints());
            //        var tanksOnPump = Bb.OurTanksSet.Where(t => pumpPoints.Contains(t.ToPoint()));
            //        if (!tanksOnPump.Any())
            //        {
            //            var openPoints = pumpPoints.Where(p => spawnable.Get(p));
            //            if (openPoints.Any())
            //            {
            //                if (players[playerID()].Oxygen >= tankCost)
            //                {
            //                    Bb.tileLookup[openPoints.First()].spawn((int)Types.Tank);
            //                }
            //                else
            //                {
            //                    shouldSpwanScouts = false;
            //                }
            //            }
            //        }
            //        if (players[playerID()].Oxygen < tankCost)
            //        {
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //}

            Bb.ReadBoard();

            var theirPumpingPumps = Bb.TheirPumpSet.Where(pump => Solver.WillBePumping(pump));
            var theirPumpingPumpsBits = theirPumpingPumps.SelectMany(p => p.GetPoints()).ToBitArray();
            var ourOwnedPumpingPumps = Bb.OurPumpSet.Where(p => Solver.WillBePumping(p));
            var ourOwnedSiegedPumpingPumps = Bb.OurPumpSet.Where(p => p.station.SiegeAmount > 0 && Solver.WillBePumping(p));
            var ourOwnedSiegedPumpingPumpsBits = ourOwnedSiegedPumpingPumps.SelectMany(p => p.GetPoints()).ToBitArray();

            var ourSpawnable = new HashSet<Point>(Bb.GetOurSpawnable().ToPoints());
            //if (shouldSpwanScouts == true)
            //{
            while ((players[playerID()].Oxygen >= scoutCost) && (ourSpawnable.Count != 0) && (Bb.TheirPumpSet.Count != 0))
            {
                var start = CalcSpawnPoint(ourSpawnable, theirPumpingPumpsBits.Or(ourOwnedSiegedPumpingPumpsBits).ToPoints());
                if (start.x == -1)
                {
                    start = CalcSpawnPoint(ourSpawnable, Bb.TheirPumps.ToPoints());
                }
                Bb.tileLookup[start].spawn((int)Types.Scout);
                ourSpawnable.Remove(start);
            }
            //}

            Bb.ReadBoard();

            // Do Stuffs For Each Unit
            foreach (Unit u in Bb.OurUnitsSet)
            {
                if (u.Type == (int)Types.Scout)
                {
                    if (theirPumpingPumps.Count() != 0 || ourOwnedSiegedPumpingPumps.Count() > 0)
                    {
                        Solver.MoveAndAttack(u, Bb.TheirScoutsSet.Union(Bb.TheirWorkersSet), true);
                        if (u.HasAttacked)
                        {
                            continue;
                        }
                        Solver.Move(u, theirPumpingPumpsBits.Or(ourOwnedSiegedPumpingPumpsBits));
                        if (u.MovementLeft == u.MaxMovement)
                        {
                            Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
                        }
                        else
                        {
                            Solver.Attack(u);
                        }
                    }
                    else if (Bb.TheirPumpSet.Count != 0)
                    {
                        Solver.Move(u, Bb.TheirPumps);
                        Solver.Attack(u);
                        finalStageCounter++;
                    }
                    else
                    {
                        Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
                        finalStageCounter++;
                    }
                }
                else if (u.Type == (int)Types.Worker)
                {
                    //Find nearest non pumping pump

                }
                else if (u.Type == (int)Types.Tank)
                {
                    if (tankReturn.ContainsKey(u.Id))
                    {
                        var r = tankReturn[u.Id];
                        if (Solver.GetPassable().Get(r))
                        {
                            u.move(r.x, r.y);
                            tankReturn.Remove(u.Id);
                        }
                        Solver.Attack(u);
                    }
                    // Check whether our pump is pumping and move/die if it isn't
                    else if (!ourOwnedPumpingPumps.SelectMany(p => p.GetPoints()).Contains(u.ToPoint()))
                    {
                        Solver.Move(u, Bb.Water, walkInWater: true);
                    }
                    else if (!Solver.Attack(u))
                    {
                        var r = u.ToPoint();
                        Solver.MoveAndAttack(u, Bb.TheirUnitsSet);
                        if (!r.Equals(u.ToPoint()))
                        {
                            tankReturn[u.Id] = r;
                        }
                    }
                }
            }
        }
        else
        {
            if (stage == 1 && BaseAI.players[Bb.usId].WaterStored <= BaseAI.players[Bb.themId].WaterStored) // Suicide
            {
                Console.WriteLine("STAGE TWO");
                foreach (Unit u in Bb.OurUnitsSet)
                {
                    Solver.Move(u, Bb.Water, true);
                }
                if (Bb.OurUnitsSet.Count == 0)
                {
                    // Suicide complete
                    stage = 2;
                }
            }
            else if (stage == 2 && BaseAI.players[Bb.usId].WaterStored <= BaseAI.players[Bb.themId].WaterStored)
            {
                // Stage two
                // Spawn 4 tanks and move them to one pump
                SpawnTanks(CalcSpawnPoint);
                MoveTanksToPump();
                // Spawn 1 worker and have him dig to our pump (avoid connecting other pumps)
                SpawnWorker(CalcSpawnPoint);
                // Have worker maintain the trench until empty
                MaintainTrench();
            }
            else
            {
                // Spawn Scouts
                SpawnScouts(CalcSpawnPoint);
                // Everyone Move/Attack
                AllMoveAttack();
            }

            if (Bb.TheirPumpSet.Where(pump => Solver.WillBePumping(pump)).Count() > 0)
            {
                stage = 1;
                finalStageCounter = 0;
            }
        }

        return true;
    }

    private void SpawnScouts(Func<IEnumerable<Point>, IEnumerable<Point>, Point> CalcSpawnPoint)
    {
        bool spawned = true;
        while (players[playerID()].Oxygen >= 12 && spawned)
        {
            Point spawnPoint;
            if (Bb.OurUnitsSet.Count > 0)
            {
                spawnPoint = CalcSpawnPoint(Bb.GetOurSpawnable().ToPoints(), Bb.OurUnits.ToPoints());
            }
            else if (Bb.TheirUnitsSet.Count > 0)
            {
                spawnPoint = CalcSpawnPoint(Bb.GetOurSpawnable().ToPoints(), Bb.TheirUnits.ToPoints());
            }
            else
            {
                spawnPoint = Bb.GetOurSpawnable().ToPoints().First();
            }
            //Console.WriteLine("Scout Spawn Point: (" + spawnPoint.x + ", " + spawnPoint.y + ")");
            spawned = Bb.tileLookup[spawnPoint].spawn((int)Types.Scout);
            Bb.ReadBoard();
        }
    }

    private void AllMoveAttack()
    {
        foreach (Unit unit in Bb.OurUnitsSet)
        {
            Solver.MoveAndAttack(unit, Bb.TheirUnitsSet);
            Bb.ReadBoard();
        }
    }

    private void MaintainTrench()
    {
        if (Bb.OurWorkersSet.Count == 1)
        {
            Unit worker = Bb.OurWorkersSet.First();
            if (finalPump.station.Owner != Bb.usId)
            {
                // Go capture it
                Console.WriteLine("Worker capturing pump");
                Solver.Move(worker, finalPump.GetBitArray());
            }
            else
            {
                // Maintain trench
                // Dig it if it doesn't exit
                // Dig shallowest otherwise
                Console.WriteLine("Worker digging holes");
                var neededPoints = Digger.GetNeededTrenches(finalPump);
                Digger.MoveAndDig(worker, neededPoints);
            }
            Bb.ReadBoard();
        }
    }

    private void SpawnWorker(Func<IEnumerable<Point>, IEnumerable<Point>, Point> CalcSpawnPoint)
    {
        if (Bb.OurWorkersSet.Count == 0 && players[playerID()].Oxygen >= 10)
        {
            var start = CalcSpawnPoint(Bb.GetOurSpawnable().ToPoints(), finalPump.GetPoints());
            Bb.tileLookup[start].spawn((int)Types.Worker);
        }
        Bb.ReadBoard();
    }

    private void MoveTanksToPump()
    {
        foreach (Unit tank in Bb.OurTanksSet)
        {
            Solver.Move(tank, finalPump.GetBitArray());
            Solver.Attack(tank);
            Bb.ReadBoard();
        }
        foreach (Unit scout in Bb.OurScoutsSet)
        {
            Solver.MoveAndAttack(scout, Bb.TheirUnitsSet);
            Bb.ReadBoard();
        }
    }

    private Pump FindFinalPump()
    {
        var pumps = Bb.OurPumpSet;
        if (!pumps.Any())
        {
            pumps = Bb.TheirPumpSet;
        }
        return pumps.minByValue(p => Bb.GlaciersSet.Min(g => p.GetPoints().Min(pc => Solver.Manhattan(pc, g))));
        
        //var allPumps = Bb.OurPumpSet.Union(Bb.TheirPumpSet).Union(Bb.NeutralPumpSet);
        //return allPumps.minByValue(pump => Bb.OurSpawnSet.Sum(spawn => Solver.Manhattan(spawn, pump.NW)));
    }

    private void SpawnTanks(Func<IEnumerable<Point>, IEnumerable<Point>, Point> CalcSpawnPoint)
    {
        if (finalPump == null)
        {
            finalPump = FindFinalPump();
        }
        int maxTanks = 4;
        if (finalPump.station.Owner != Bb.usId)
        {
            maxTanks = 3;
        }
        bool spawned = true;
        while (players[playerID()].Oxygen >= 15 && Bb.OurTanksSet.Count < maxTanks && spawned)
        {
            var spawnPoint = CalcSpawnPoint(Bb.GetOurSpawnable().ToPoints(), finalPump.GetPoints());
            if (finalPump.GetPoints().Contains(spawnPoint))
            {
                spawned = Bb.tileLookup[spawnPoint].spawn((int)Types.Tank);
            }
            else
            {
                spawned = Bb.tileLookup[spawnPoint].spawn((int)Types.Scout);
            }
            Bb.ReadBoard();
        }
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
