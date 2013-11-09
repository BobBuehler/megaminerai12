using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CSClient
{
    // Pump station is multiple tiles, need list of pump station bitboards
    /* Bit Boards */
    public static class Bb
    {
        public static int maxX; // (0, 0) is the top-left corner
        public static int maxY;
        public static BitArray Glaciers;
        public static BitArray Trenches;
        public static BitArray OurPumps;
        public static BitArray TheirPumps;
        public static BitArray Pumps;
        public static BitArray OurSpawns;
        public static BitArray TheirSpawns;
        public static BitArray Water;

        public static void Init(AI ai)
        {
            maxX = ai.mapWidth();
            maxY = ai.mapHeight();
            int size = maxX * maxY;
            int us = ai.playerID();
            int them = 1-ai.playerID();

            Glaciers = new BitArray(size);
            Trenches = new BitArray(size);
            OurPumps = new BitArray(size);
            TheirPumps = new BitArray(size);
            Pumps = new BitArray(size);
            OurSpawns = new BitArray(size);
            TheirSpawns = new BitArray(size);
            Water = new BitArray(size);

            foreach (Tile tile in BaseAI.tiles)
            {
                int offset = GetOffset(tile.X, tile.Y);
                // Trenches
                if (tile.Depth > 0)
                {
                    Trenches[offset] = true;
                }
                // Water
                if (tile.WaterAmount > 0)
                {
                    Water[offset] = true;
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
                // OurSpawns, TheirSpawns
                if (tile.Owner == us)
                {
                    OurSpawns[offset] = true;
                }
                else if(tile.Owner == them)
                {
                    TheirSpawns[offset] = true;
                }
                
            }
        }

        public static int GetOffset(int x, int y)
        {
            return y * maxX + x;
        }
    }
}
