using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class CompetitionSolver : BaseNegaMaxSolver
    {
        public static readonly int reachableCellsThreshold = 10;

        protected override void Evaluate(SearchNode searchNode, GameState gameStateAfterMove)
        {
            if (gameStateAfterMove.IsGameOver)
            {
                if (gameStateAfterMove.PlayerToMoveNext == PlayerType.You)
                {
                    searchNode.Evaluation = double.NegativeInfinity;
                }
                else
                {
                    searchNode.Evaluation = double.PositiveInfinity;
                }
            }
            else
            {
                double evaluation;
                if (gameStateAfterMove.OpponentIsInSameCompartment)
                {
                    evaluation = EvaluateWhenInSameCompartment(gameStateAfterMove);
                }
                else
                {
                    evaluation = EvaluateWhenInSeparateCompartments(gameStateAfterMove);
                }
                searchNode.Evaluation = evaluation;
            }
        }

        private double EvaluateWhenInSameCompartment(GameState gameStateAfterMove)
        {
            double VALUE_OF_A_CONTROLLED_SQUARE = 10000.0;

            int closestCellsDifference = gameStateAfterMove.NumberOfCellsClosestToYou - gameStateAfterMove.NumberOfCellsClosestToOpponent;
            int degreesOfClosestCellsDifference = gameStateAfterMove.TotalDegreesOfCellsClosestToYou - gameStateAfterMove.TotalDegreesOfCellsClosestToOpponent;
            
            int yourDistanceFromNearestPole = 1000;
            int opponentsDistanceFromNearestPole = 1000;
            bool youHaveAccessToAPole = false;
            bool opponentHasAccessToAPole = false;

            if (gameStateAfterMove.NorthPole.CompartmentStatus == CompartmentStatus.InSharedCompartment 
                || gameStateAfterMove.NorthPole.CompartmentStatus == CompartmentStatus.InYourCompartment)
            {
                yourDistanceFromNearestPole = gameStateAfterMove.NorthPole.DistanceFromYou;
                youHaveAccessToAPole = true;
            }

            if (gameStateAfterMove.NorthPole.CompartmentStatus == CompartmentStatus.InSharedCompartment 
                || gameStateAfterMove.NorthPole.CompartmentStatus == CompartmentStatus.InOpponentsCompartment)
            {
                opponentsDistanceFromNearestPole = gameStateAfterMove.NorthPole.DistanceFromOpponent;
                opponentHasAccessToAPole = true;
            }

            if (gameStateAfterMove.SouthPole.CompartmentStatus == CompartmentStatus.InSharedCompartment 
                || gameStateAfterMove.SouthPole.CompartmentStatus == CompartmentStatus.InYourCompartment)
            {
                youHaveAccessToAPole = true;
                int distanceFromSouthPole = gameStateAfterMove.SouthPole.DistanceFromYou;
                if (distanceFromSouthPole < yourDistanceFromNearestPole)
                {
                    yourDistanceFromNearestPole = distanceFromSouthPole;
                }
            }

            if (gameStateAfterMove.SouthPole.CompartmentStatus == CompartmentStatus.InSharedCompartment 
                || gameStateAfterMove.SouthPole.CompartmentStatus == CompartmentStatus.InOpponentsCompartment)
            {
                opponentHasAccessToAPole = true;
                int distanceFromSouthPole = gameStateAfterMove.SouthPole.DistanceFromOpponent;
                if (distanceFromSouthPole < opponentsDistanceFromNearestPole)
                {
                    opponentsDistanceFromNearestPole = distanceFromSouthPole;
                }
            }

            // When in a shared compartment, being far from a pole is good, because:
            // 1. The search tree lookahead is severely limited near the poles.
            // 2. You will be more likely to reach the pole last, in which case you can choose whether to be on Odd or Even parity squares,
            //    So if there is a charge down a narrow channel at the end, you will have a good chance of winning.
            // 3. However not being near a pole is very bad, particularly as we get close to the end game.
            //    Evaluate this as 1 reachable cell being lost for every 20 moves taken (so after around 800 turns, this is equivalent to a 40 cell difference).
            int combinedMoveNumber = gameStateAfterMove.YourCell.MoveNumber + gameStateAfterMove.OpponentsCell.MoveNumber;

            double valueToYouOfDistanceFromPole = youHaveAccessToAPole ? yourDistanceFromNearestPole : -0.05 * combinedMoveNumber;
            double valueToOpponentOfDistanceFromPole = opponentHasAccessToAPole ? opponentsDistanceFromNearestPole : -0.05 * combinedMoveNumber;
            double distanceFromPoleValueDifference = valueToYouOfDistanceFromPole - valueToOpponentOfDistanceFromPole;

            // However, if neither of you has access to a pole, but you are on a different parity cell from your opponent after your move, then it is very good!
            double valueOfCorrectParity = 0.0;
            if ((!youHaveAccessToAPole) && (!opponentHasAccessToAPole))
            {
                Parity yourParity = gameStateAfterMove.YourCell.Parity;
                Parity opponentsParity = gameStateAfterMove.OpponentsCell.Parity;

                if (yourParity != Parity.Neutral && opponentsParity != Parity.Neutral)
                {
                    // Treat it as being worth 0 controlled squares near the beginning of the game, but 100 squares near the end:
                    valueOfCorrectParity = 100 * VALUE_OF_A_CONTROLLED_SQUARE * combinedMoveNumber / 842.0;

                    bool inYourFavour = false;

                    if (gameStateAfterMove.PlayerToMoveNext == PlayerType.You)
                    {
                        // You are about to move. You want to move onto a different parity from your opponent. So you want to have the same parity now.
                        inYourFavour = yourParity == opponentsParity;
                    }
                    else
                    {
                        // Conversely, if your opponent is about to move, you want him to have to move onto the same colour square as you
                        inYourFavour = yourParity != opponentsParity;
                    }

                    if (!inYourFavour)
                    {
                        valueOfCorrectParity = -valueOfCorrectParity;
                    }
                }
            }

            double differenceInValueOfBeingFarFromPole = opponentsDistanceFromNearestPole - yourDistanceFromNearestPole;

            return VALUE_OF_A_CONTROLLED_SQUARE * closestCellsDifference + 100.0 * degreesOfClosestCellsDifference + distanceFromPoleValueDifference + valueOfCorrectParity;
        }

        private double EvaluateWhenInSeparateCompartments(GameState gameStateAfterMove)
        {
            int reachableCellsDifference = gameStateAfterMove.NumberOfCellsReachableByYou - gameStateAfterMove.NumberOfCellsReachableByOpponent;
            int degreesOfReachableCellsDifference = gameStateAfterMove.TotalDegreesOfCellsReachableByYou - gameStateAfterMove.TotalDegreesOfCellsReachableByOpponent;
            int componentBranchesDifference = gameStateAfterMove.NumberOfComponentBranchesInYourTree - gameStateAfterMove.NumberOfComponentBranchesInOpponentsTree;
            double evaluation = 10000 * reachableCellsDifference + 100 * degreesOfReachableCellsDifference - 10 * componentBranchesDifference;

            // Encourage closing opponent into a separate smaller area, rather than keeping things open:
            if (reachableCellsDifference >= reachableCellsThreshold)
            {
                evaluation = 1000 * evaluation;
            }

            return evaluation;
        }

    }
}
