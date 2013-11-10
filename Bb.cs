using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

// Dustin needs hashet of spawnable tiles
// ./run r99acm.device.mst.edu <gamenum>
// Pump station is multiple tiles, need list of pump station bitboards
/* Bit Boards */
public static class Bb
{
    public static int maxX; // (0, 0) is the top-left corner
    public static int maxY;
    public static int size;
    public static int usId;
    public static int themId;

    public static BitArray OurPumps;
    public static BitArray TheirPumps;
    public static BitArray NeutralPumps;
    public static BitArray OurTiles;
    public static BitArray TheirTiles;
    public static BitArray OurSpawns;
    public static BitArray TheirSpawns;
    public static BitArray Glaciers;
    public static BitArray Trenches;
    public static BitArray Water;
    public static BitArray OurUnits;
    public static BitArray TheirUnits;

    public static HashSet<Point> OurPumpSet;
    public static HashSet<Point> TheirPumpSet;
    public static HashSet<Point> NeutralPumpSet; // May be able to ignore?
    public static HashSet<Point> OurSpawnSet;
    public static HashSet<Point> TheirSpawnSet;
    public static HashSet<Point> GlaciersSet;
    public static HashSet<Point> TrenchesSet;
    public static HashSet<Point> WaterSet;

    public static HashSet<Unit> OurUnitsSet;
    public static HashSet<Unit> TheirUnitsSet;
    public static HashSet<Unit> OurWorkersSet;
    public static HashSet<Unit> TheirWorkersSet;
    public static HashSet<Unit> OurScoutsSet;
    public static HashSet<Unit> TheirScoutsSet;
    public static HashSet<Unit> OurTanksSet;
    public static HashSet<Unit> TheirTanksSet;

    private static bool init = false;

    public static void Init(AI ai)
    {
        maxX = ai.mapWidth() - 1;
        maxY = ai.mapHeight() - 1;
        size = (maxX + 1) * (maxY + 1);
        usId = ai.playerID();
        themId = 1 - ai.playerID();

        Reset();

        init = true;

        ReadBoard();
    }


    public static void ReadBoard()
    {
        if (!init)
        {
            throw new Exception("Must call Init(AI ai) before using ReadBoard()");
        }
        Reset();

        foreach (Tile tile in BaseAI.tiles)
        {
            int offset = GetOffset(tile.X, tile.Y);
            Point point = new Point(tile.X, tile.Y);

            // Trenches
            if (tile.Depth > 0 && tile.WaterAmount == 0)
            {
                Trenches[offset] = true;
                TrenchesSet.Add(point);
            }

            // Water
            if (tile.WaterAmount > 0 && tile.Depth > 0)
            {
                Water[offset] = true;
                WaterSet.Add(point);
            }

            // Glaciers
            if (tile.Depth == 0 && tile.WaterAmount > 0 && tile.Owner == 3)
            {
                Glaciers[offset] = true;
                GlaciersSet.Add(point);
            }

            // Pumping Stations
            if (tile.PumpID != -1)
            {
                foreach (PumpStation station in BaseAI.pumpStations)
                {
                    if (station.Id == tile.PumpID)
                    {
                        if (station.Owner == usId)
                        {
                            OurPumps[offset] = true;
                            OurPumpSet.Add(point);
                        }
                        else if (station.Owner == themId)
                        {
                            TheirPumps[offset] = true;
                            TheirPumpSet.Add(point);
                        }
                        else
                        {
                            NeutralPumps[offset] = true;
                            NeutralPumpSet.Add(point);
                        }
                    }
                }
            }

            // Spawn Tiles
            if (tile.Owner == usId)
            {
                OurTiles[offset] = true;
                OurSpawnSet.Add(point);
            }
            else if (tile.Owner == themId)
            {
                TheirTiles[offset] = true;
                TheirSpawnSet.Add(point);
            }
        }
        Console.WriteLine("Looking through units now.");
        // All Units
        foreach (Unit unit in BaseAI.units)
        {
            Point point = new Point(unit.X, unit.Y);
            int offset = GetOffset(point);

            // Our Units
            if (unit.Owner == usId)
            {
                OurUnitsSet.Add(unit);
                OurUnits[offset] = true;
                if (unit.Type == 0) // Worker
                {
                    OurWorkersSet.Add(unit);
                }
                else if (unit.Type == 1) // Scout
                {
                    OurScoutsSet.Add(unit);
                }
                else if (unit.Type == 2) // Tank
                {
                    OurTanksSet.Add(unit);
                }
                if (OurSpawnSet.Contains(point))
                {
                    OurSpawnSet.Remove(point);
                }
            }

            // Their Units
            if (unit.Owner == themId)
            {
                TheirUnitsSet.Add(unit);
                TheirUnits[offset] = true;
                if (unit.Type == 0) // Worker
                {
                    TheirWorkersSet.Add(unit);
                }
                else if (unit.Type == 1) // Scout
                {
                    TheirScoutsSet.Add(unit);
                }
                else if (unit.Type == 2) // Tank
                {
                    TheirTanksSet.Add(unit);
                }
                if (TheirSpawnSet.Contains(point))
                {
                    TheirSpawnSet.Remove(point);
                }
            }
        }

        string str = "Empty";
        if (OurUnitsSet.Count > 0)
        {
            str = "";
            foreach (Unit u in OurUnitsSet)
            {
                str += u.Type + " ";
            }
        }
        Console.WriteLine("Our Units: " + str);
    }



    private static void Reset()
    {
        Glaciers = new BitArray(size);
        Trenches = new BitArray(size);
        OurPumps = new BitArray(size);
        TheirPumps = new BitArray(size);
        NeutralPumps = new BitArray(size);
        OurSpawns = new BitArray(size);
        TheirSpawns = new BitArray(size);
        Water = new BitArray(size);
        OurUnits = new BitArray(size);
        TheirUnits = new BitArray(size);
        OurTiles = new BitArray(size);
        TheirTiles = new BitArray(size);

        OurPumpSet = new HashSet<Point>();
        TheirPumpSet = new HashSet<Point>();
        NeutralPumpSet = new HashSet<Point>();
        OurSpawnSet = new HashSet<Point>(); // Any tile where we can currently spawn a unit
        TheirSpawnSet = new HashSet<Point>();
        GlaciersSet = new HashSet<Point>();
        TrenchesSet = new HashSet<Point>();
        WaterSet = new HashSet<Point>();

        OurUnitsSet = new HashSet<Unit>();
        TheirUnitsSet = new HashSet<Unit>();
        OurWorkersSet = new HashSet<Unit>();
        TheirWorkersSet = new HashSet<Unit>();
        OurScoutsSet = new HashSet<Unit>();
        TheirScoutsSet = new HashSet<Unit>();
        OurTanksSet = new HashSet<Unit>();
        TheirTanksSet = new HashSet<Unit>();
    }

    public static int GetOffset(int x, int y)
    {
        return y * maxX + x;
    }

    public static int GetOffset(Point p)
    {
        return p.y * maxX + p.x;
    }

    public static bool IsPumping(Point m)
    {
        //station
        throw new NotImplementedException();
    }

    // Is this pump pumping; start = every tile under pump; isGoal = is it a glacier?; passable = pump OR water OR glacier; c: (c, n) => 1; h: c => 0
}
