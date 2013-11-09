using System;
using System.Runtime.InteropServices;

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
        spawnUnits();

        int moveDelta = 0;

        // Set to move left or right based on ID; towards the center.
        moveDelta = playerID() == 0 ? 1 : -1;

        // Do some stuff for each unit.
        for (int i = 0; i < units.Length; i++)
        {
            // If you don't own the unit, ignore it.
            if (units[i].Owner != playerID())
                continue;

            moveUnit(i, moveDelta);

            // If there's an enemy in the movement direction and the unit hasn't attacked and is alive.
            if (!units[i].HasAttacked && units[i].HealthLeft > 0)
            {
                for (int j = 0; j < units.Length; j++)
                {
                    // Check if there is a enemy unit in the direction.
                    if (units[i].X + moveDelta == units[j].X && units[i].Y == units[j].Y &&
                      units[j].Owner != playerID())
                    {
                        // Attack it!
                        units[i].attack(units[j]);
                        break;
                    }
                }
            }

            // If there's a space to dig below the unit and the unit hasn't dug, and the unit is alive.
            if (units[i].Y != mapHeight() - 1 &&
              tiles[units[i].X * mapHeight() + units[i].Y + 1].PumpID == -1 &&
              tiles[units[i].X * mapHeight() + units[i].Y + 1].Owner == 2 &&
              units[i].HasDug == false &&
              units[i].HealthLeft > 0)
            {
                bool canDig = true;

                // Make sure there's no unit on that tile.
                for (int j = 0; j < units.Length; j++)
                    if (units[i].X == units[j].X && units[i].Y + 1 == units[j].Y)
                        canDig = false;

                // Make sure the tile is not an ice tile.
                if (canDig && !(tiles[units[i].X * mapHeight() + units[i].Y + 1].Owner == 3 &&
                  tiles[units[i].X * mapHeight() + units[i].Y + 1].WaterAmount > 0))
                {
                    units[i].dig(tiles[units[i].X * mapHeight() + units[i].Y + 1]);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Spawn Units
    /// </summary>
    void spawnUnits()
    {
        // ...and if we can spawn more units...
        if (Bb.OurUnits.Count < maxUnits())
        {
            //Spwan Logic
            int ourOxy = players[playerID()].Oxygen;

            int workerCost = Int32.MaxValue;
            int scoutCost = Int32.MaxValue;
            int tankCost = Int32.MaxValue;

            // Get the unit cost for a worker.
            for (int j = 0; j < unitTypes.Length; j++)
                if (unitTypes[j].Type == (int)Types.Worker)
                    workerCost = unitTypes[j].Cost;
            // Get the unit cost for a scout.
            for (int j = 0; j < unitTypes.Length; j++)
                if (unitTypes[j].Type == (int)Types.Scout)
                    scoutCost = unitTypes[j].Cost;
            // Get the unit cost for a tank.
            for (int j = 0; j < unitTypes.Length; j++)
                if (unitTypes[j].Type == (int)Types.Tank)
                    tankCost = unitTypes[j].Cost;

            int numOurWorkers = Bb.OurWorkers.Count;
            int numOurScouts = Bb.OurScouts.Count;
            int numOurTanks = Bb.OurTanks.Count;
            int numOurUnits = Bb.OurUnits.Count;

            int numTheirWorkers = Bb.TheirWorkers.Count;
            int numTheirScouts = Bb.TheirScouts.Count;
            int numTheirTanks = Bb.TheirTanks.Count;
            int numTheirUnits = Bb.TheirUnits.Count;

            // Go through their pumps and pick one
            foreach (Point i in Bb.TheirPumps)
            {
                // Go through spawnable tiles
                foreach (Point j in Bb.OurSpawnSet)
                {
                    // If there is enough oxygen to spawn the unit...
                    if (ourOxy >= scoutCost)
                    {
                        tiles[Bb.GetOffset(j.x, j.y)].spawn((int)Types.Scout);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Move Units
    /// </summary>
    void moveUnit(int i, int moveDelta)
    {
        // Try to move to the right or left movement times.
        for (int z = 0; z < units[i].MaxMovement; z++)
        {
            bool canMove = true;

            // If there's a unit there, don't move.
            for (int j = 0; j < units.Length; j++)
            {
                if (units[i].X + moveDelta == units[j].X && units[i].Y == units[j].Y)
                    canMove = false;
            }

            // If nothing is there, and it's not moving off the edge of the map...
            if (canMove && units[i].X + moveDelta >= 0 && units[i].X + moveDelta < mapWidth())
            {
                // If the tile is not an enemy spawn point...
                if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].PumpID == -1 &&
                  tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 1 - playerID()) ||
                  tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 2)
                {
                    // If the tile is not an ice tile...
                    if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 3 &&
                      tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].WaterAmount > 0))
                    {
                        // If the tile is not spawning anything...
                        if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].IsSpawning))
                        {
                            // If the unit is alive...
                            if (units[i].HealthLeft > 0)
                            {
                                // Move the unit!
                                units[i].move(units[i].X + moveDelta, units[i].Y);
                            }
                        }
                    }
                }
            }
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
