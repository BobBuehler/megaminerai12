using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSClient
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override int GetHashCode()
        {
            return Y * 20 + X;
        }

        public override bool Equals(object obj)
        {
            Point other = (Point)obj;
            return (X == other.X) && (Y == other.Y);
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }
    }
}
