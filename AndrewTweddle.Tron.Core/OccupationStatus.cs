using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public enum OccupationStatus : byte
    {
        Clear = 0,
        You = 1,
        YourWall = 2,
        Opponent = 3,
        OpponentWall = 4
    }
}
