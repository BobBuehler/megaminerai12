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
    public static int us;
    public static int them;

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

    public static void Init(AI ai)
    {
        maxX = ai.mapWidth();
        maxY = ai.mapHeight();
        us = ai.playerID();
        them = 1 - ai.playerID();

        Reset();

        ReadBoard();
    }

    public static void ReadBoard()
    {
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
            if (tile.WaterAmount > 0 && tile.Owner != 3 && tile.Depth > 0)
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
                        if (station.Owner == us)
                        {
                            OurPumps[offset] = true;
                            OurPumpSet.Add(point);
                        }
                        else if (station.Owner == them)
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
            if (tile.Owner == us)
            {
                OurTiles[offset] = true;
            }
            else if (tile.Owner == them)
            {
                TheirTiles[offset] = true;
            }

            OurSpawns = OurTiles.Or(OurPumps).And(TheirUnits.Not()).And(OurUnits.Not());
        }
    }

    private static void Reset()
    {
        int size = maxX * maxY;
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

    public static bool IsPumping(Point m)
    {
        //station
        throw new NotImplementedException();
    }

    // Is this pump pumping; start = every tile under pump; isGoal = is it a glacier?; passable = pump OR water OR glacier; c: (c, n) => 1; h: c => 0
}
