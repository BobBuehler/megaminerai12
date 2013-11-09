using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CSClient
{
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

        public static HashSet<Point> OurPumpSet;
        public static HashSet<Point> TheirPumpSet;
        public static HashSet<Point> NeutralPumpSet;
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

            OurPumpSet = new HashSet<Point>();
            TheirPumpSet = new HashSet<Point>();
            NeutralPumpSet = new HashSet<Point>();
            OurSpawnSet = new HashSet<Point>(); // Any tile where we can currently spawn a unit
            TheirSpawnSet = new HashSet<Point>();
            GlaciersSet = new HashSet<Point>();
            TrenchesSet = new HashSet<Point>();
            WaterSet = new HashSet<Point>();

            OurUnits = new HashSet<Unit>();
            TheirUnits = new HashSet<Unit>();
            OurWorkers = new HashSet<Unit>();
            TheirWorkers = new HashSet<Unit>();
            OurScouts = new HashSet<Unit>();
            TheirScouts = new HashSet<Unit>();
            OurTanks = new HashSet<Unit>();
            TheirTanks = new HashSet<Unit>();

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
                    OurPumpSet.Add(point);
                    // Add something to the spawnset
                }
                else if (tile.PumpID == them)
                {
                    TheirPumps[offset] = true;
                    TheirPumpSet.Add(point);
                    // Add something to the spawnset
                }
                else if (tile.PumpID != -1)
                {
                    NeutralPumps[offset] = true;
                    NeutralPumpSet.Add(point);
                }
                // OurSpawns, TheirSpawns
                if (tile.Owner == us)
                {
                    OurSpawns[offset] = true;
                    OurSpawnSet.Add(point);
                }
                else if (tile.Owner == them)
                {
                    TheirSpawns[offset] = true;
                    TheirSpawnSet.Add(point);
                }
                if (tile.Owner == 3 && tile.WaterAmount > 0)
                {
                    Glaciers[offset] = true;
                }
            }


        }

        public static void ReadBoard()
        {
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
                if (tile.WaterAmount > 0 && tile.Owner != 3)
                {
                    Water[offset] = true;
                    WaterSet.Add(point);
                }
            }
        }

        public static int GetOffset(int x, int y)
        {
            return y * maxX + x;
        }

        public static bool IsPumping(this PumpStation station)
        {
            //station
            throw new NotImplementedException();
        }

        // Is this pump pumping; start = every tile under pump; isGoal = is it a glacier?; passable = pump OR water OR glacier; c: (c, n) => 1; h: c => 0
    }
}
