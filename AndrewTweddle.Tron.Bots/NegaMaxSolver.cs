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

        public bool IncludeDegreeOfVertexOfPoles
        {
            get;
            set;
        }

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
                int totalDegreesDifference = gameStateAfterMove.TotalDegreesOfCellsClosestToYou - gameStateAfterMove.TotalDegreesOfCellsClosestToOpponent;

                if (!IncludeDegreeOfVertexOfPoles)
                {
                    // Exclude the poles, since they have a large number of edges, which causes the algorithm to avoid filling the poles:
                    CellState[] poles = new CellState[] { gameStateAfterMove.NorthPole, gameStateAfterMove.SouthPole };
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
                }

                int totalDifferenceInClosestCells = gameStateAfterMove.NumberOfCellsClosestToYou - gameStateAfterMove.NumberOfCellsClosestToOpponent;
                int totalReachableCellsDifference = gameStateAfterMove.NumberOfCellsReachableByYou - gameStateAfterMove.NumberOfCellsReachableByOpponent;

                if (gameStateAfterMove.OpponentIsInSameCompartment || totalReachableCellsDifference < reachableCellsThreshold)
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
