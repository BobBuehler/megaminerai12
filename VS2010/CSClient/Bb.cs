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
    public static BitArray OurPumps;
    public static BitArray TheirPumps;
    public static BitArray NeutralPumps;
    public static BitArray OurSpawns;
    public static BitArray TheirSpawns;
    public static BitArray Glaciers;
    public static BitArray Trenches;
    public static BitArray Water;

    public static HashSet<PumpStation> OurPumpSet;
    public static HashSet<PumpStation> TheirPumpSet;
    public static HashSet<PumpStation> NeutralPumpSet;
    public static HashSet<Point> OurSpawnSet;
    public static HashSet<Point> TheirSpawnSet;
    public static HashSet<Point> GlaciersSet;
    public static HashSet<Point> TrenchesSet;
    public static HashSet<Point> WaterSet;

    public static HashSet<Unit> OurUnits;
    public static HashSet<Unit> TheirUnits;
    public static HashSet<Unit> OurWorkers;
    public static HashSet<Unit> TheirWorkers;
    public static HashSet<Unit> OurScouts;
    public static HashSet<Unit> TheirScouts;
    public static HashSet<Unit> OurTanks;
    public static HashSet<Unit> TheirTanks;

    public static void Init(AI ai)
    {
        maxX = ai.mapWidth();
        maxY = ai.mapHeight();
        int size = maxX * maxY;
        int us = ai.playerID();
        int them = 1 - ai.playerID();

        Glaciers = new BitArray(size);
        Trenches = new BitArray(size);
        OurPumps = new BitArray(size);
        TheirPumps = new BitArray(size);
        NeutralPumps = new BitArray(size);
        OurSpawns = new BitArray(size);
        TheirSpawns = new BitArray(size);
        Water = new BitArray(size);

        OurPumpSet = new HashSet<PumpStation>();
        TheirPumpSet = new HashSet<PumpStation>();
        NeutralPumpSet = new HashSet<PumpStation>();
        OurSpawnSet = new HashSet<Point>();
        TheirSpawnSet = new HashSet<Point>();
        GlaciersSet = new HashSet<Point>();
        TrenchesSet = new HashSet<Point>();
        WaterSet = new HashSet<Point>();

        OurUnits = new HashSet<Unit>();
        TheirUnits = new HashSet<Unit>();

        foreach (Tile tile in BaseAI.tiles)
        {
            int offset = GetOffset(tile.X, tile.Y);
            Point point = new Point(tile.X, tile.Y);
            // Trenches
            if (tile.Depth > 0)
            {
                Trenches[offset] = true;
                TrenchesSet.Add(point);
            }
            // Water
            if (tile.WaterAmount > 0)
            {
                Water[offset] = true;
                WaterSet.Add(point);
            }
            // OurPumps, TheirPumps
            if (tile.PumpID == us)
            {
                OurPumps[offset] = true;
            }
            else if (tile.PumpID == them)
            {
                TheirPumps[offset] = true;
            }
            else if (tile.PumpID != -1)
            {
                NeutralPumps[offset] = true;
            }
            // OurSpawns, TheirSpawns
            if (tile.Owner == us)
            {
                OurSpawns[offset] = true;
            }
            else if (tile.Owner == them)
            {
                TheirSpawns[offset] = true;
            }
            if (tile.Owner == 3 && tile.WaterAmount > 0)
            {
                Glaciers[offset] = true;
            }
        }
    }

    public static int GetOffset(int x, int y)
    {
        return y * maxX + x;
    }
}