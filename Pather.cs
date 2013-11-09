using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class Pather
{
    public static IEnumerable<Point> AStar(IEnumerable<Point> starts, Func<Point, bool> isGoal, BitArray passable, Func<Point, Point, int> c, Func<Point, int> h)
    {
        //    closedset := the empty set    // The set of nodes already evaluated.
        var closedset = new HashSet<Point>();
        //    openset := {start}    // The set of tentative nodes to be evaluated, initially containing the start node
        var openset = new HashSet<Point>(starts);
        //    came_from := the empty map    // The map of navigated nodes.
        var came_from = new Dictionary<Point, Point>();

        //    g_score[start] := 0    // Cost from start along best known path.
        var g_score = new Dictionary<Point, int>();
        starts.ForEach(s => g_score[s] = 0);
        //    // Estimated total cost from start to goal through y.
        //    f_score[start] := g_score[start] + heuristic_cost_estimate(start, goal)
        var f_score = new Dictionary<Point, int>();
        starts.ForEach(s => f_score[s] = h(s));

        //    while openset is not empty
        while (openset.Count > 0)
        {
            //        current := the node in openset having the lowest f_score[] value
            var current = openset.minByValue(p => f_score[p]);
            //        if current = goal
            if (isGoal(current))
            {
                //    if current_node in came_from
                //        p := reconstruct_path(came_from, came_from[current_node])
                //        return (p + current_node)
                //    else
                //        return current_node
                var path = new LinkedList<Point>();
                path.AddFirst(current);
                while (came_from.ContainsKey(current))
                {
                    current = came_from[current];
                    path.AddFirst(current);
                }
                return path;
            }

            //        remove current from openset
            openset.Remove(current);
            //        add current to closedset
            closedset.Add(current);
            //        for each neighbor in neighbor_nodes(current)
            foreach (Point neighbor in GetNeighbors(current, passable))
            {
                //            tentative_g_score := g_score[current] + dist_between(current,neighbor)
                var gs = g_score[current] + c(current, neighbor);
                //            tentative_f_score := tentative_g_score + heuristic_cost_estimate(neighbor, goal)
                var fs = gs + h(neighbor);
                //            if neighbor in closedset and tentative_f_score >= f_score[neighbor]
                if (closedset.Contains(neighbor) && fs >= f_score[neighbor])
                    //                    continue
                    continue;

                //            if neighbor not in openset or tentative_f_score < f_score[neighbor] 
                //                came_from[neighbor] := current
                came_from[neighbor] = current;
                //                g_score[neighbor] := tentative_g_score
                g_score[neighbor] = gs;
                //                f_score[neighbor] := tentative_f_score
                f_score[neighbor] = fs;
                //                if neighbor not in openset
                //                    add neighbor to openset
                openset.Add(neighbor);
            }
        }

        //    return failure
        return null;
    }

    public static IEnumerable<Point> GetNeighbors(Point p, BitArray passable)
    {
        Point[] neighbors =
            {
                new Point(p.x - 1, p.y),
                new Point(p.x + 1, p.y),
                new Point(p.x, p.y - 1),
                new Point(p.x, p.y + 1)
            };

        return neighbors.Where(n => n.x >= 0 && n.x < Bb.maxX && n.y >= 0 && n.y < Bb.maxY && passable.Get(n));
    }
}
