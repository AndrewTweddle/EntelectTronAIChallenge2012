using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public static class Helper
    {
        public static OccupationStatus ToOccupationStatus(this PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Opponent:
                    return OccupationStatus.Opponent;

                case PlayerType.You:
                    return OccupationStatus.You;

                default:
                    throw new ArgumentException("PlayerType.Unknown can not be converted to an OccupationStatus", "playerType");
            }
        }

        public static OccupationStatus ToWallType(this PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Opponent:
                    return OccupationStatus.OpponentWall;

                case PlayerType.You:
                    return OccupationStatus.YourWall;

                default:
                    throw new ArgumentException("PlayerType.Unknown can not be converted to a wall OccupationStatus", "playerType");
            }
        }

        public static int NormalizedX(int x)
        {
            if (x <= -1)
            {
                x = x % Constants.Columns;
                if (x < 0)
                {
                    x += Constants.Columns;
                }
            }
            if (x >= Constants.Columns)
            {
                return x % Constants.Columns;
            }
            return x;
        }
    }
}
