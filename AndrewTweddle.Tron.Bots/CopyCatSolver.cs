using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Core.Algorithms;

namespace AndrewTweddle.Tron.Bots
{
    public class CopyCatSolver: BaseSolver
    {
        protected override void DoSolve()
        {
            if (Coordinator.CurrentGameState.PlayerWhoMovedFirst == PlayerType.You)
            {
                throw new ApplicationException("Copycat bot must go second!");
            }

            GameState gameState = Coordinator.CurrentGameState.Clone();

            int midlineX = (gameState.OpponentsOriginalCell.Position.X + gameState.YourOriginalCell.Position.X) / 2;
            int opponentsXOffset = gameState.OpponentsCell.Position.X - midlineX;
            int newX = (midlineX + 1 - opponentsXOffset + Constants.Columns) % Constants.Columns;
            int newY = Constants.SouthPoleY - gameState.OpponentsCell.Position.Y;

            gameState.MoveToPosition(new Position(newX, newY));
            Coordinator.SetBestMoveSoFar(gameState);
        }
    }
}
