using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TronSdk;
using AndrewTweddle.Tron.Core;
using System.Reflection;
using System.IO;

namespace AndrewTweddle.Tron.BotAdapter
{
    public abstract class BaseBotAdapter: ITronBot
    {
        protected abstract ISolver CreateSolver();  // Factory method

        public ISolver Solver { get; private set; }

        protected BaseBotAdapter()
        {
            Solver = CreateSolver();
        }
        
        public void ExecuteMove(ref BlockTypes[,] grid)
        {
            /* Prepare: */
            Coordinator coordinator = new Coordinator(Solver);
            coordinator.StartTime = DateTime.Now;

            /* Load the data: */
            IEnumerable<RawCellData> cells = ConvertGridToRawCellData(grid);

            /* Run the solver: */
            coordinator.Run(cells);
            
            lock (coordinator.BestMoveLock)
            {
                /* Save game state: */
                coordinator.SaveGameState();

                /* Communicate decision back to the marshall: */
                UpdateGridWithNewGameState(ref grid, coordinator.BestMoveSoFar);
            }
        }

        private IEnumerable<RawCellData> ConvertGridToRawCellData(BlockTypes[,] grid)
        {
            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = 0; y < Constants.Rows; y++)
                {
                    BlockTypes blockType = grid[x, y];
                    OccupationStatus occupationStatus = (OccupationStatus)Enum.Parse(typeof(OccupationStatus), blockType.ToString()); 
                    yield return new RawCellData
                    {
                        X = x,
                        Y = y,
                        OccupationStatus = occupationStatus
                    };
                }
            }
        }

        private void UpdateGridWithNewGameState(ref BlockTypes[,] grid, GameState newGameState)
        {
            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = 0; y < Constants.Rows; y++)
                {
                    CellState cellState = newGameState[x, y];
                    BlockTypes blockType = (BlockTypes)Enum.Parse(typeof(BlockTypes), cellState.OccupationStatus.ToString());
                    grid[x, y] = blockType;
                }
            }
        }
    }
}
