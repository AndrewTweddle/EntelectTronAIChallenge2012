using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public class MoveInALine<TState>: Step<TState> where TState: class
    {
        public Direction Direction { get; set; }
        public int Distance { get; set; }
        public Func<int, int> GetDistanceFromLevel { get; set; }

        public override RuleOutcome ReplayAndChooseNextMove(GameState gameStateBeforeStep, GameState currentGameState)
        {
            int distance = Distance;
            int xOffset = GetFlippedXOffset(Direction.GetXOffset());
            int yOffset = GetFlippedYOffset(Direction.GetYOffset());

            if (GetDistanceFromLevel != null)
            {
                distance = GetDistanceFromLevel(Level);
            }

            Position nextPosition = gameStateBeforeStep.YourCell.Position;
            int nextMoveNumber = gameStateBeforeStep.YourCell.MoveNumber;

            for (int i = 0; i < distance; i++)
            {
                // Calculate move number, position and cell for the next move:
                nextMoveNumber++;
                int newX = GetNormalizedX(nextPosition.X + xOffset);
                int newY = nextPosition.Y + yOffset;
                nextPosition = new Position(newX, newY);

                RuleOutcome outcome = MoveToNextPosition(gameStateBeforeStep, currentGameState, nextPosition, nextMoveNumber);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }
            }

            // The move in a line has been completed:
            return new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };
        }
    }
}
