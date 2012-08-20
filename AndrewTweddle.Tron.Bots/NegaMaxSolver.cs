using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class NegaMaxSolver: BaseNegaMaxSolver
    {
        public static readonly int reachableCellsThreshold = 50;

        protected override void Evaluate(SearchNode searchNode)
        {
            if (!searchNode.GameState.GetPossibleNextStates().Any())
            {
                if (searchNode.GameState.PlayerToMoveNext == PlayerType.You)
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
                int totalDegreesdifference = searchNode.GameState.TotalDegreesOfCellsClosestToYou - searchNode.GameState.TotalDegreesOfCellsClosestToOpponent;
                int totalDifferenceInClosestCells = searchNode.GameState.NumberOfCellsClosestToYou - searchNode.GameState.NumberOfCellsClosestToOpponent;
                int totalReachableCellsDifference = searchNode.GameState.NumberOfCellsReachableByYou - searchNode.GameState.NumberOfCellsReachableByOpponent;

                if (searchNode.GameState.OpponentIsInSameCompartment || totalReachableCellsDifference < reachableCellsThreshold)
                {
                    searchNode.Evaluation = totalDegreesdifference + totalDifferenceInClosestCells;
                }
                else
                {
                    // Encourage closing opponent into a separate smaller area:
                    searchNode.Evaluation = 1000 * (totalDegreesdifference + totalDifferenceInClosestCells);
                }
            }
        }
    }
}
