using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public static class DirectionHelper
    {
        public static int GetXOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return -1;
                case Direction.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int GetYOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return -1;
                case Direction.Down:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
