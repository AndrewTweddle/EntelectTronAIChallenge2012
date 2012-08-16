using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TronSdk
{
    /// <summary>
    /// An integer point object
    /// </summary>
    public class Point
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }


    }
}
