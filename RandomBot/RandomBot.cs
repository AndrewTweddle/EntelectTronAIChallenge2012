using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TronSdk;

namespace TronBot1
{
    public class RandomBot : ITronBot
    {

        public void ExecuteMove(ref BlockTypes[,] grid)
        {
            Point location = Common.FindLocation(grid);

            Random rnd = new Random(DateTime.Now.Millisecond);

            while (true)
            {
                int moveIndex = rnd.Next(4);
                Point newPoint = null;

                //don't allow left/right moves on polar rows
                if ((location.Y == 0) || (location.Y == Common.BOARD_BLOCKS_Y - 1))
                {
                    if ((moveIndex == Common.MOVE_INDEX_LEFT) || (moveIndex == Common.MOVE_INDEX_RIGHT))
                        continue;

                    int y = Common.BOARD_BLOCKS_Y - 2;

                    if (location.Y == 0)
                    {
                        y = 1;
                    }

                    int xStartPos = rnd.Next(Common.BOARD_BLOCKS_X - 1);

                    for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
                    {
                        Point point = new Point((x + xStartPos) % Common.BOARD_BLOCKS_X, y);
                        if (grid[point.X, point.Y] == BlockTypes.Clear)
                        {
                            newPoint = point;
                            break;
                        }
                    }
                }
                else
                {
                    newPoint = Common.GetNewPoint(location, moveIndex);
                }
                if (newPoint != null)
                {
                    if (grid[newPoint.X, newPoint.Y] == BlockTypes.Clear)
                    {
                        grid[location.X, location.Y] = BlockTypes.YourWall;
                        grid[newPoint.X, newPoint.Y] = BlockTypes.You;

                        //set the whole polar row to You
                        if ((newPoint.Y == 0) || (newPoint.Y == Common.BOARD_BLOCKS_Y - 1))
                        {
                            for (int i = 0; i < Common.BOARD_BLOCKS_X; i++)
                                grid[i, newPoint.Y] = BlockTypes.You;
                        }

                        //set the whole polar row to YourWall
                        if ((location.Y == 0) || (location.Y == Common.BOARD_BLOCKS_Y - 1))
                        {
                            for (int i = 0; i < Common.BOARD_BLOCKS_X; i++)
                                grid[i, location.Y] = BlockTypes.YourWall;
                        }
                        return;
                    }
                }
            }
        }
    }
}
