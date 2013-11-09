using System;
using System.Runtime.InteropServices;
using System.Collections;
using CSClient;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Bb.maxX = 5;
            Bb.maxY = 5;

            Point[] starts = { new Point() };
            Func<Point, bool> isGoal = p => p.x == 4 && p.y == 4;
            BitArray passable = new BitArray(new bool[] {
                true, false, true, true, true,
                true, true, true, false, true,
                true, true, true, false, true,
                true, false, false, false, true,
                true, true, true, true, true,
            });

            var path = Pather.AStar(starts, isGoal, passable, (c, n) => n.x == 0 ? 1 : 0, p => 0);
            foreach (Point p in path)
            {
                Console.WriteLine(p);
            }
        }

        if (args.Length < 1)
        {
            System.Console.WriteLine("Please enter a hostname");
            return;
        }

        IntPtr connection = Client.createConnection();

        AI ai = new AI(connection);
        if (Client.serverConnect(connection, args[0], "19000") == 0)
        {
            System.Console.WriteLine("Unable to connect to server");
            return;
        }

        if (Client.serverLogin(connection, ai.username(), ai.password()) == 0)
            return;

        if (args.Length < 2)
            Client.createGame(connection);
        else
            Client.joinGame(connection, Int32.Parse(args[1]), "player");

        while (Client.networkLoop(connection) != 0)
        {
            if (ai.startTurn())
                Client.endTurn(connection);
            else
                Client.getStatus(connection);
        }

        Client.networkLoop(connection); //Grab end game state
        Client.networkLoop(connection); //Grab log
        ai.end();
    }
}
