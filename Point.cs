﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode()
    {
        return y * 20 + x;
    }

    public override bool Equals(object obj)
    {
        Point other = (Point)obj;
        return (x == other.x) && (y == other.y);
    }

    public override string ToString()
    {
        return String.Format("({0},{1})", x, y);
    }
}