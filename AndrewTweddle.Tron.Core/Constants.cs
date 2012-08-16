using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public static class Constants
    {
        public static readonly int Rows = 30;
        public static readonly int Columns = 30;
        public static readonly int SouthPoleX = 0;
        public static readonly int SouthPoleY = Rows - 1;
        public static readonly int NorthPoleX = 0;
        public static readonly int NorthPoleY = 0;
        public static readonly int ArcticCircleY = NorthPoleY + 1;
        public static readonly int AntarcticCircleY = SouthPoleY - 1;
    }
}
