using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

// Dustin needs hashet of spawnable tiles
// ./run r99acm.device.mst.edu <gamenum>
// bit board: is spawning
// bit board: is under siege
/* Bit Boards */
public static class Bb
{
    public static int maxX; // (0, 0) is the top-left corner
    public static int maxY;
    public static int size;
    public static int usId;
    public static int themId;

    public static BitArray Truth;

    public static BitArray Glaciers;
    public static BitArray Trenches;
    public static BitArray Water;
    public static BitArray OurSpawns;
    public static BitArray TheirSpawns;
    public static BitArray OurUnits;
    public static BitArray TheirUnits;
    public static BitArray OurPumps;
    public static BitArray TheirPumps;
    public static BitArray NeutralPumps;
    public static BitArray UnderSiege;
    public static BitArray IsSpawning;

    public static HashSet<Point> OurSpawnSet;
    public static HashSet<Point> TheirSpawnSet;
    public static HashSet<Point> GlaciersSet;

    public static HashSet<Unit> OurUnitsSet;
    public static HashSet<Unit> TheirUnitsSet;
    public static HashSet<Unit> OurWorkersSet;
    public static HashSet<Unit> TheirWorkersSet;
    public static HashSet<Unit> OurScoutsSet;
    public static HashSet<Unit> TheirScoutsSet;
    public static HashSet<Unit> OurTanksSet;
    public static HashSet<Unit> TheirTanksSet;

    public static HashSet<Pump> OurPumpSet;
    public static HashSet<Pump> TheirPumpSet;
    public static HashSet<Pump> NeutralPumpSet; // May be able to ignore?

    public static Dictionary<Point, Tile> tileLookup;
    public static Dictionary<Point, Pump> pumpPointLookup;

    private static bool init = false;

    public static void Init(AI ai)
    {
        maxX = ai.mapWidth() - 1;
        maxY = ai.mapHeight() - 1;
        size = (maxX + 1) * (maxY + 1);
        usId = ai.playerID();
        themId = 1 - ai.playerID();

        Truth = new BitArray(size).Not();

        Reset();

        init = true;

        ReadBoard();
    }

    private static void Print(HashSet<Point> points)
    {
        string str = "Empty";
        if (points.Count > 0)
        {
            str = "";
            foreach (Point p in points)
            {
                str += "(" + p.x + ", " + p.y + ") ";
            }
        }
        Console.WriteLine(str);
    }

    private static void Print(Tile tile)
    {
        string str = "";
        str += "Tile (" + tile.X + ", " + tile.Y + ")";
        str += "\nOwner: " + tile.Owner;
        str += "\nID: " + tile.Id;
        str += "\nDepth: " + tile.Depth;
        str += "\nWater: " + tile.WaterAmount;
        str += "\nPumpID: " + tile.PumpID;
        Console.WriteLine(str + "\n");
    }

    public static void ReadBoard()
    {
        if (!init)
        {
            throw new Exception("Must call Init(AI ai) before using ReadBoard()");
        }
        Reset();

        tileLookup = BaseAI.tiles.ToDictionary(t => t.ToPoint());

        foreach (Tile tile in BaseAI.tiles)
        {
            int offset = GetOffset(tile.X, tile.Y);
            Point point = new Point(tile.X, tile.Y);

            // IsSpawning
            if (tile.IsSpawning)
            {
                IsSpawning[offset] = true;
            }

            // Glaciers
            if (tile.Depth == 0 && tile.WaterAmount > 0 && tile.Owner == 3)
            {
                Glaciers[offset] = true;
                GlaciersSet.Add(point);
            }

            // Trenches
            if (tile.Depth > 0 && tile.WaterAmount == 0)
            {
                Trenches[offset] = true;
            }

            // Water
            if (tile.Depth > 0 && tile.WaterAmount > 0)
            {
                Water[offset] = true;
            }

            // Our Spawns
            if (tile.Owner == usId && tile.PumpID == -1)
            {
                OurSpawns[offset] = true;
                OurSpawnSet.Add(point);
            }

            // Their Spawns
            if (tile.Owner == themId && tile.PumpID == -1)
            {
                TheirSpawns[offset] = true;
                TheirSpawnSet.Add(point);
            }

            // Our Pumps
            if (tile.Owner == usId && tile.PumpID != -1)
            {
                OurPumps[offset] = true;
            }

            // Their Pumps
            if (tile.Owner == themId && tile.PumpID != -1)
            {
                TheirPumps[offset] = true;
            }

            // Neutral Pumps
            if (tile.Owner == 2 && tile.PumpID != -1)
            {
                NeutralPumps[offset] = true;
            }
        }

        // Units
        foreach (Unit unit in BaseAI.units)
        {
            Point point = new Point(unit.X, unit.Y);
            int offset = GetOffset(point);

            // Our Units
            if (unit.Owner == usId)
            {
                OurUnits[offset] = true;
                OurUnitsSet.Add(unit);
                switch (unit.Type)
                {
                    case 0: OurWorkersSet.Add(unit); break;
                    case 1: OurScoutsSet.Add(unit); break;
                    case 2: OurTanksSet.Add(unit); break;
                }
            }

            // Their Units
            if (unit.Owner == themId)
            {
                TheirUnits[offset] = true;
                TheirUnitsSet.Add(unit);
                switch (unit.Type)
                {
                    case 0: TheirWorkersSet.Add(unit); break;
                    case 1: TheirScoutsSet.Add(unit); break;
                    case 2: TheirTanksSet.Add(unit); break;
                }
            }
        }

        var pumps = new BitArray(OurPumps).Or(TheirPumps).Or(NeutralPumps);
        var pumpLookup = BaseAI.pumpStations.ToDictionary(p => p.Id);
        foreach (var p in pumps.ToPoints())
        {
            var tile = tileLookup[p];
            if (!pumpLookup.ContainsKey(tile.PumpID))
            {
                continue;
            }
            var ps = pumpLookup[tile.PumpID];
            pumpLookup.Remove(ps.Id);

            var pump = new Pump(ps, p);
            if (pump.station.SiegeAmount > 0)
            {
                UnderSiege[GetOffset(pump.SW.x, pump.SW.y)] = true;
                UnderSiege[GetOffset(pump.NW.x, pump.NW.y)] = true;
                UnderSiege[GetOffset(pump.SE.x, pump.SE.y)] = true;
                UnderSiege[GetOffset(pump.NE.x, pump.NE.y)] = true;
            }
            if (tile.Owner == usId)
            {
                OurPumpSet.Add(pump);
            }
            else if (tile.Owner == themId)
            {
                TheirPumpSet.Add(pump);
            }
            else
            {
                NeutralPumpSet.Add(pump);
            }
        }

        pumpPointLookup = OurPumpSet.Union(TheirPumpSet).Union(NeutralPumpSet)
            .SelectMany(pump => pump.GetPoints().Select(point => new { ump = pump, oint = point }))
            .ToDictionary(a => a.oint, a => a.ump);
    }

    public static BitArray GetOurSpawnable()
    {
        // (spawns + pumps-not-under-seige) - units - spawning
        var spawns = Bb.OurSpawnSet.ToBitArray();
        var pumps = Bb.OurPumpSet.Where(p => p.station.SiegeAmount == 0).SelectMany(p => p.GetPoints()).ToBitArray();
        var units = new BitArray(Bb.OurUnits).Or(Bb.TheirUnits);
        var spawning = new BitArray(Bb.IsSpawning);
        return spawns.Or(pumps).And(units.Not()).And(spawning.Not());
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
        UnderSiege = new BitArray(size);
        IsSpawning = new BitArray(size);

        OurSpawnSet = new HashSet<Point>(); // Any tile where we can currently spawn a unit
        TheirSpawnSet = new HashSet<Point>();
        GlaciersSet = new HashSet<Point>();

        OurPumpSet = new HashSet<Pump>();
        TheirPumpSet = new HashSet<Pump>();
        NeutralPumpSet = new HashSet<Pump>();

        OurUnitsSet = new HashSet<Unit>();
        TheirUnitsSet = new HashSet<Unit>();
        OurWorkersSet = new HashSet<Unit>();
        TheirWorkersSet = new HashSet<Unit>();
        OurScoutsSet = new HashSet<Unit>();
        TheirScoutsSet = new HashSet<Unit>();
        OurTanksSet = new HashSet<Unit>();
        TheirTanksSet = new HashSet<Unit>();

        tileLookup = new Dictionary<Point, Tile>();
        pumpPointLookup = new Dictionary<Point, Pump>();
    }

    public static int GetOffset(int x, int y)
    {
        return y * (maxX + 1) + x;
    }

    public static int GetOffset(Point p)
    {
        return GetOffset(p.x, p.y);
    }

    public static bool IsPumping(Point m)
    {
        //station
        throw new NotImplementedException();
    }

    // Is this pump pumping; start = every tile under pump; isGoal = is it a glacier?; passable = pump OR water OR glacier; c: (c, n) => 1; h: c => 0
}
