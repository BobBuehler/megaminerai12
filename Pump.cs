using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class Pump
{
    public PumpStation station;
    public Point NW;
    public Point NE;
    public Point SW;
    public Point SE;

    public Pump(PumpStation station, Point nw)
    {
        this.station = station;
        NW = nw;
        NE = new Point(nw.x + 1, nw.y);
        SW = new Point(nw.x, nw.y + 1);
        SE = new Point(nw.x + 1, nw.y + 1);
    }

    public IEnumerable<Point> GetPoints()
    {
        yield return NW;
        yield return NE;
        yield return SW;
        yield return SE;
    }

    public BitArray GetBitArray()
    {
        return GetPoints().ToBitArray();
    }
}
