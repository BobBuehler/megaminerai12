using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CSClient
{
    /* Bit Boards */
    public static class Bb
    {
        public static int maxX;
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

            Glaciers = new BitArray(maxX * maxY);
            Trenches = new BitArray(maxX * maxY);
            OurPumps = new BitArray(maxX * maxY);
            TheirPumps = new BitArray(maxX * maxY);
            Pumps = new BitArray(maxX * maxY);
            OurSpawns = new BitArray(maxX * maxY);
            TheirSpawns = new BitArray(maxX * maxY);
            Water = new BitArray(maxX * maxY);
        }
    }
}
