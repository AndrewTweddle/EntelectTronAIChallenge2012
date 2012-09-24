using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public static class RulesEngineHelper
    {
        public static GameState GetInitialGameState(GameState currentGameState)
        {
            GameState gameState = new GameState();
            CellState yourOriginalCell = gameState[currentGameState.YourOriginalCell.Position];
            CellState yourCell = yourOriginalCell;
            gameState.YourWallLength = 0;
            CellState opponentsOriginalCell = gameState[currentGameState.OpponentsOriginalCell.Position];
            CellState opponentsCell;

            if (currentGameState.PlayerWhoMovedFirst == PlayerType.You)
            {
                opponentsCell = opponentsOriginalCell;
                opponentsCell.OccupationStatus = OccupationStatus.Opponent;
                gameState.OpponentsWallLength = 0;
            }
            else
            {
                CellState opponentsFirstCellInCurrentGameState
                    = currentGameState.GetNextCellStateForPlayerFromPreviousCellState(currentGameState.OpponentsOriginalCell);
                if (opponentsFirstCellInCurrentGameState == null)
                {
                    throw new ApplicationException(
                        "The opponents first move could not be determined. Hence the rules engine could not determine the initial game state.");
                }
                opponentsCell = gameState[opponentsFirstCellInCurrentGameState.Position];
                opponentsCell.MoveNumber = 1;
                opponentsCell.OccupationStatus = OccupationStatus.Opponent;
                opponentsOriginalCell.OccupationStatus = OccupationStatus.OpponentWall;
                gameState.OpponentsWallLength = 1;
            }
            yourCell.OccupationStatus = OccupationStatus.You;

            gameState.YourOriginalCell = yourOriginalCell;
            gameState.YourCell = yourCell;
            gameState.OpponentsOriginalCell = opponentsOriginalCell;
            gameState.OpponentsCell = opponentsCell;
            gameState.PlayerWhoMovedFirst = currentGameState.PlayerWhoMovedFirst;
            gameState.PlayerToMoveNext = PlayerType.You;

            // Don't run shortest path & biconnected components algorithms. Each step can decide whether it needs this or not.
            // TODO: Provide an overridable method to do this.

            return gameState;
        }

        /// <summary>
        /// A helper method for moving through a sequence of points for a particular step
        /// </summary>
        /// <param name="gameStateBeforeStep"></param>
        /// <param name="currentGameState"></param>
        /// <param name="nextPosition"></param>
        /// <param name="nextMoveNumber"></param>
        /// <returns></returns>
        public static RuleOutcome MoveToNextPosition(GameState gameStateBeforeStep, GameState currentGameState, Position nextPosition, int nextMoveNumber)
        {
            CellState actualCellState = currentGameState[nextPosition];

            // Check whether the correct move was made to this position:
            OccupationStatus statusOfNextPosition = actualCellState.OccupationStatus;

            switch (statusOfNextPosition)
            {
                case OccupationStatus.YourWall:
                case OccupationStatus.You:
                    // Check move number:
                    if (actualCellState.MoveNumber != nextMoveNumber)
                    {
                        return new RuleOutcome
                        {
                            Status = RuleStatus.DeviationFromRuleDetected
                        };
                    }
                    break;

                case OccupationStatus.Opponent:
                case OccupationStatus.OpponentWall:
                    return new RuleOutcome
                    {
                        Status = RuleStatus.DeviationFromRuleDetected
                    };
                case OccupationStatus.Clear:
                    return new RuleOutcome
                    {
                        Status = RuleStatus.NextMoveSuggested,
                        SuggestedMove = new Move(PlayerType.You, nextMoveNumber, nextPosition)
                    };
            }

            // Apply chosen move:
            gameStateBeforeStep.MoveToPosition(nextPosition);

            // Apply opponent's move:
            ApplyOpponentsMoveToMutablePosition(gameStateBeforeStep, currentGameState, nextMoveNumber);

            return new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };
        }

        private static void ApplyOpponentsMoveToMutablePosition(GameState gameStateBeforeStep, GameState currentGameState, int yourLastMoveNumber)
        {
            int opponentsMoveNumber = currentGameState.PlayerWhoMovedFirst == PlayerType.You ? yourLastMoveNumber : yourLastMoveNumber + 1;
            CellState opponentsActualMove = currentGameState.GetCellByMoveNumber(PlayerType.Opponent, opponentsMoveNumber);
            gameStateBeforeStep.MoveToPosition(opponentsActualMove.Position);
        }

        public static RuleOutcome MoveToPositionAlongAnyRoute(GameState mutableGameState, GameState actualGameState, ref int nextMoveNumber, Position targetPosition)
        {
            RuleOutcome outcome;
            bool isTargetReachable = mutableGameState.IsCellStateReachableByPlayer(PlayerType.You, mutableGameState[targetPosition]);
            List<Position> positionsOnRouteToTarget = mutableGameState.GetPositionsOnAnyRouteToTheTargetPosition(PlayerType.You, targetPosition);
            foreach (Position pos in positionsOnRouteToTarget)
            {
                nextMoveNumber++;
                outcome = RulesEngineHelper.MoveToNextPosition(mutableGameState, actualGameState, pos, nextMoveNumber);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }
            }
            outcome = new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };

            return outcome;
        }
    }
}
