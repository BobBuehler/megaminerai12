using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class Pump
{
    PumpStation station;
    Point NW;
    Point NE;
    Point SW;
    Point SE;

    public Pump(PumpStation station, Point nw)
    {
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
        var bits = new BitArray(Bb.size);
        GetPoints().ForEach(p => bits.Set(p, true));
        return bits;
    }
}
