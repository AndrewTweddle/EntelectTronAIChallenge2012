using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TronSdk
{
    public static class Common
    {
        public const int BOARD_BLOCKS_X = 30;
        public const int BOARD_BLOCKS_Y = 30;

        public const int MOVE_INDEX_RIGHT = 0;
        public const int MOVE_INDEX_UP = 1;
        public const int MOVE_INDEX_LEFT = 2;
        public const int MOVE_INDEX_DOWN = 3;

        /// <summary>
        /// Emits a text representation of the board (use for low-level debugging).
        /// Important: In keeping with 3D standards, the up vector on the viewport = -1. This means that y decreases upwards
        /// instead of downwards. Because of this, the grid returned by GridToString will appear upside down
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GridToString(BlockTypes[,] state)
        {
            string ret = "";

            for (int y = 0; y < BOARD_BLOCKS_Y; y++)
            {
                for (int x = 0; x < BOARD_BLOCKS_X; x++)
                {
                    switch (state[x,y]){
                        case BlockTypes.Clear: ret += "."; break;
                        case BlockTypes.You: ret += "X"; break;
                        case BlockTypes.YourWall: ret += "x"; break;
                        case BlockTypes.Opponent: ret += "O"; break;
                        case BlockTypes.OpponentWall: ret += "o"; break;
                    }

                }
                ret += "\r\n";
            }

            return ret;
        }

        /// <summary>
        /// Returns an (X,Y) coordinate by offsetting the specified location with either 1 unit up, left, down or right
        /// </summary>
        /// <param name="location"></param>
        /// <param name="moveIndex"></param>
        /// <returns>Point object. Null indicates that the point reference is not valid</returns>
        public static Point GetNewPoint(Point location, int moveIndex)
        {
            Point newPoint = new Point(0, 0);

            //move
            switch (moveIndex)
            {
                case MOVE_INDEX_RIGHT: newPoint = new Point(location.X + 1, location.Y); break;
                case MOVE_INDEX_UP: newPoint = new Point(location.X, location.Y - 1); break;
                case MOVE_INDEX_LEFT: newPoint = new Point(location.X - 1, location.Y); break;
                case MOVE_INDEX_DOWN: newPoint = new Point(location.X, location.Y + 1); break;
                default:
                    throw new Exception("Invalid move index!");
            }

            //wrap
            if (newPoint.X < 0) newPoint.X = TronSdk.Common.BOARD_BLOCKS_X - 1;
            if (newPoint.X >= TronSdk.Common.BOARD_BLOCKS_X) newPoint.X = 0;
            
            if (newPoint.Y < 0) return null;
            if (newPoint.Y >= TronSdk.Common.BOARD_BLOCKS_Y) return null;

            return newPoint;
        }

        /// <summary>
        /// Returns the location of the first instance of the BlockType.You block
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static Point FindLocation(BlockTypes[,] grid)
        {
            return FindLocation(BlockTypes.You, grid);
        }
        
        /// <summary>
        /// Returns the location of the first instance of BlockType find
        /// </summary>
        /// <param name="find"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static Point FindLocation(BlockTypes find, BlockTypes[,] grid)
        {
            for (int x = 0; x < TronSdk.Common.BOARD_BLOCKS_X; x++)
            {
                for (int y = 0; y < TronSdk.Common.BOARD_BLOCKS_Y; y++)
                {
                    if (grid[x, y] == find)
                        return new Point(x, y);

                }
            }

            throw new Exception("Not present!");
        }

    }
}
