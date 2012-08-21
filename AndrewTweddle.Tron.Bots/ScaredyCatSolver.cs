using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Core.Algorithms;

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
            gameState.MoveToPosition(new Position(newX, newY));
            Coordinator.SetBestMoveSoFar(gameState);
        }
    }
}
