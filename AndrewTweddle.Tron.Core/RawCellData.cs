using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public struct RawCellData
    {
        public OccupationStatus OccupationStatus
        {
            get;
            set;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
