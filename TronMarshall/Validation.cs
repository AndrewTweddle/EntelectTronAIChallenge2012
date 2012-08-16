using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TronSdk;

namespace TronMarshall
{
    static class Validation
    {
        /// <summary>
        /// Validates that the player 'You' has performed only valid moves. This method must always be called before the 
        /// players are flipped. If a rule violation is found, an exception is thrown
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public static void ValidateMove(BlockTypes[,] oldState, BlockTypes[,] newState)
        {
            if (newState == null)
                throw new Exception("No grid was passed back!");

            if ((newState.GetLength(0) != Common.BOARD_BLOCKS_X) || (newState.GetLength(1) != Common.BOARD_BLOCKS_Y))
                throw new Exception("An incorrect grid size was returned!");

            int headBlockCount = 0;
            int headBlockMinRow = Common.BOARD_BLOCKS_Y + 1;
            int headBlockMaxRow = -1;

            for (int y = 0; y < Common.BOARD_BLOCKS_Y; y++)
            {
                for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
                {

                    if (oldState[x, y] != newState[x, y])
                    {
                        switch (newState[x, y])
                        {
                            case BlockTypes.You:
                                if (oldState[x, y] != BlockTypes.Clear)
                                    throw new Exception("Head moved onto a block that was not clear!");

                                headBlockCount++;
                                if (y < headBlockMinRow) headBlockMinRow = y;
                                if (y > headBlockMaxRow) headBlockMaxRow = y;
                                break;

                            case BlockTypes.YourWall:
                                //in the prior state, these blocks must be You. we don't need to do anything
                                //more since in the prior state, updating these blocks to You will have already been validated
                                if (oldState[x, y] != BlockTypes.You)
                                    throw new Exception("Bot left a wall where it hasn't previously moved!");

                                break;

                            default:
                                throw new Exception("Bot changed a block that should not have been changed!");
                        }
                    }

                    //check that each You block is adjacent to a wall block
                    if (newState[x, y] == BlockTypes.You)
                    {
                        bool ok = false;

                        if ((y == 0) || (y == Common.BOARD_BLOCKS_Y - 1))
                        {
                            //check that row immediately before/after the polar row has at least 1 wall
                            for (int i = 0; i < Common.BOARD_BLOCKS_X; i++)
                            {
                                if (newState[i, y == 0 ? 1 : Common.BOARD_BLOCKS_Y - 2] == BlockTypes.YourWall)
                                {
                                    ok = true;
                                    break;
                                }
                            }
                        }
                        else
                        {

                            for (int i = 0; i < 4; i++)
                            {
                                Point point = Common.GetNewPoint(new Point(x, y), i);
                                if ((point != null) && (newState[point.X, point.Y] == BlockTypes.YourWall))
                                {
                                    ok = true;
                                    break;
                                }
                            }
                        }

                        if (!ok)
                            throw new Exception("Bot head moved more than 1 unit away from wall!");

                    }
                }

            }

            if (headBlockCount == 0)
                throw new Exception("Bot head is missing!");

            if (headBlockMinRow != headBlockMaxRow)
                throw new Exception("Bot head spans multiple rows!");

            //only allowed on polar rows and if so, the count must be the full grid width.
            //nb: we've already validated that min=max
            if ((headBlockMinRow == 0) || (headBlockMinRow == Common.BOARD_BLOCKS_Y - 1))
            {
                if (headBlockCount != Common.BOARD_BLOCKS_X)
                    throw new Exception("On polar rows, the entire row must contain the head block!");
            }
            else if (headBlockCount > 1)
            {
                throw new Exception("Only 1 head block is allowed except on polar rows!");
            }

        }

        public static bool IsEndOfGame(BlockTypes[,] state, out int winnerBot)
        {
            TronSdk.Point bot1Location = Common.FindLocation(BlockTypes.You, state);
            TronSdk.Point bot2Location = Common.FindLocation(BlockTypes.Opponent, state);

            if (!HasValidMoves(bot1Location, state))
            {
                winnerBot = 2;
                return true;
            }

            if (!HasValidMoves(bot2Location, state))
            {
                winnerBot = 1;
                return true;
            }

            winnerBot = 0;
            return false;
        }

        public static bool HasValidMoves(TronSdk.Point location, BlockTypes[,] state)
        {
            bool isAtPole = false;
            int yNearPole = 1;

            if (location.Y == 0)
            {
                isAtPole = true;
            }

            if (location.Y == 29)
            {
                isAtPole = true;
                yNearPole = 28;
            }

            if (isAtPole)
            {
                for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
                {
                    TronSdk.Point point = new Point(x, yNearPole);
                    if (state[point.X, point.Y] == BlockTypes.Clear)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    TronSdk.Point point = TronSdk.Common.GetNewPoint(location, i);
                    if ((point != null) && (state[point.X, point.Y] == BlockTypes.Clear))
                        return true;
                }

                return false;
            }
        }
    }
}
