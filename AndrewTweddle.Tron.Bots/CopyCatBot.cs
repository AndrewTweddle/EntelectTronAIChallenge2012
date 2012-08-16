using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class CopyCatBot
    {
        public GameState GenerateNextGameState(GameState gameState)
        {
            if (gameState.PlayerWhoMovedFirst == PlayerType.You)
            {
                throw new ApplicationException("Copycat bot must go second!");
            }

            int midlineX = (gameState.OpponentsOriginalCell.Position.X + gameState.YourOriginalCell.Position.X) / 2;
            int opponentsXOffset = gameState.OpponentsCell.Position.X - midlineX;
            int newX = (midlineX + 1 - opponentsXOffset + Constants.Columns) % Constants.Columns;
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

            return gameState;
        }
    }
}
