using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public static class RegexConstants
    {
        public static readonly string TronGameFileRegularExpression = @"^\s*(?<X>\d+)\s+(?<Y>\d+)\s+(?<OccupationStatus>Clear|YourWall|You|OpponentWall|Opponent)\s*$";
        public static readonly string XCaptureGroupName = "X";
        public static readonly string YCaptureGroupName = "Y";
        public static readonly string OccupationStatusCaptureGroupName = "OccupationStatus";
    }
}
