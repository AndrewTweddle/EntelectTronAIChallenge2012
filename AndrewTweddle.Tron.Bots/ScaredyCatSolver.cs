using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class ScaredyCatSolver: BaseSolver
    {
        protected override void DoSolve()
        {
            GameState gameState = Coordinator.CurrentGameState.Clone();

            if (gameState.PlayerWhoMovedFirst == PlayerType.You)
            {
                throw new ApplicationException("Scaredy cat bot must go second!");
            }

            int newX = (gameState.OpponentsCell.Position.X + Constants.Columns / 2) % Constants.Columns;
            int newY = Constants.SouthPoleY - gameState.OpponentsCell.Position.Y;

            gameState.YourCell.OccupationStatus = OccupationStatus.YourWall;

            CellState yourNewCell = gameState[newX, newY];

            if (yourNewCell.OccupationStatus != OccupationStatus.Clear)
            {
                string errorMessage = String.Format(
                    "Error attempting to move to point ({0}, {1}) - point is already occuped", yourNewCell.Position.X, yourNewCell.Position.Y);
                throw new ApplicationException(errorMessage);
            }

            yourNewCell.OccupationStatus = OccupationStatus.You;

            Coordinator.SetBestMoveSoFar(gameState);
        }
    }
}
