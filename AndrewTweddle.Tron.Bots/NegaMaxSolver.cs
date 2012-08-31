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
            if (searchNode.GameState.IsGameOver)
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
                int totalDegreesDifference = searchNode.GameState.TotalDegreesOfCellsClosestToYou - searchNode.GameState.TotalDegreesOfCellsClosestToOpponent;

                // Exclude the poles, since they have a large number of edges, which causes the algorithm to avoid filling the poles:
                CellState[] poles = new CellState[]{ searchNode.GameState.NorthPole, searchNode.GameState.SouthPole };
                foreach (CellState pole in poles)
                {
                    if (pole.OccupationStatus == OccupationStatus.Clear)
                    {
                        switch (pole.ClosestPlayer)
                        {
                            case PlayerType.You:
                                totalDegreesDifference -= pole.DegreeOfVertex;
                                break;
                            case PlayerType.Opponent:
                                totalDegreesDifference += pole.DegreeOfVertex;
                                break;
                        }
                    }
                }

                int totalDifferenceInClosestCells = searchNode.GameState.NumberOfCellsClosestToYou - searchNode.GameState.NumberOfCellsClosestToOpponent;
                int totalReachableCellsDifference = searchNode.GameState.NumberOfCellsReachableByYou - searchNode.GameState.NumberOfCellsReachableByOpponent;

                if (searchNode.GameState.OpponentIsInSameCompartment || totalReachableCellsDifference < reachableCellsThreshold)
                {
                    searchNode.Evaluation = totalDegreesDifference + totalDifferenceInClosestCells;
                }
                else
                {
                    // Encourage closing opponent into a separate smaller area:
                    searchNode.Evaluation = 1000 * (totalDegreesDifference + totalDifferenceInClosestCells);
                }
            }
        }
    }
}
