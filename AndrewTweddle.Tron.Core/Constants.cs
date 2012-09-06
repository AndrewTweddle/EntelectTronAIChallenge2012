using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public static class Constants
    {
        public const int Rows = 30;
        public const int Columns = 30;
        public const int SouthPoleX = 0;
        public const int SouthPoleY = Rows - 1;
        public const int NorthPoleX = 0;
        public const int NorthPoleY = 0;
        public const int ArcticCircleY = NorthPoleY + 1;
        public const int AntarcticCircleY = SouthPoleY - 1;
    }
}
