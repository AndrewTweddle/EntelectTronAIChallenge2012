using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core.Algorithms;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public class MoveToAPoint<TState>: Step<TState> 
        where TState : class
    {
        public Position To { get; set; }

        public Func<Step<TState>, GameState, GameState, Position> GetPosition { get; set; }

        protected Position GetFlippedTo(Position to)
        {
            int newX = GetFlippedX(to.X);
            int newY = GetFlippedY(to.Y);
            Position newTo = new Position(newX, newY);
            return newTo;
        }

        public override RuleOutcome ReplayAndChooseNextMove(GameState gameStateBeforeStep, GameState currentGameState)
        {
            Position to = To;
            if (GetPosition != null)
            {
                to = GetPosition(this, gameStateBeforeStep, currentGameState);
            }
            to = GetFlippedTo(to);

            Dijkstra.Perform(gameStateBeforeStep);
            CellState destinationState = gameStateBeforeStep[to];
            int distanceFromYou = destinationState.DistanceFromYou;

            List<Position> positionsOnRoute = new List<Position>();

            if (destinationState.OccupationStatus == OccupationStatus.Clear
                && (destinationState.CompartmentStatus == CompartmentStatus.InYourCompartment 
                    || destinationState.CompartmentStatus == CompartmentStatus.InSharedCompartment))
            {
                for (int dist = distanceFromYou - 1; dist > 0; dist--)
                {
                    int d = dist;
                    destinationState = destinationState.GetAdjacentCellStates().Where(
                        cs => cs.OccupationStatus == OccupationStatus.Clear && cs.DistanceFromYou == d).FirstOrDefault();
                    positionsOnRoute.Insert(0, destinationState.Position);
                }

                int nextMoveNumber = gameStateBeforeStep.YourCell.MoveNumber;
                foreach (Position nextPosition in positionsOnRoute)
                {
                    nextMoveNumber++;
                    RuleOutcome outcome = MoveToNextPosition(gameStateBeforeStep, currentGameState, nextPosition, nextMoveNumber);
                    if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                    {
                        return outcome;
                    }
                }

                return new RuleOutcome
                {
                    Status = RuleStatus.NoFurtherSuggestions
                };
            }
            else
            {
                return new RuleOutcome
                {
                    Status = RuleStatus.OpponentBlockedMove
                };
            }
        }
    }
}
