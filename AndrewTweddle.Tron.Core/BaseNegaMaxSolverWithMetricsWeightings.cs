using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseNegaMaxSolverWithMetricsWeightings: BaseNegaMaxSolver
    {
        public MetricsWeightings WeightingsInSameCompartment
        {
            get;
            protected set;
        }

        public MetricsWeightings WeightingsInSeparateCompartment
        {
            get;
            protected set;
        }

        protected override void Evaluate(SearchNode searchNode)
        {
            GameState gameState = searchNode.GameState;
            if (gameState.IsGameOver)
            {
                if (gameState.PlayerToMoveNext == PlayerType.You)
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
                MetricsWeightings weightings = gameState.OpponentIsInSameCompartment ? WeightingsInSameCompartment : WeightingsInSeparateCompartment;
                double evaluation
                    = weightings.NumberOfCellsReachableByPlayerFactor
                        * (gameState.NumberOfCellsReachableByYou - gameState.NumberOfCellsReachableByOpponent)
                    + weightings.TotalDegreesOfCellsReachableByPlayerFactor
                        * (gameState.TotalDegreesOfCellsReachableByYou - gameState.TotalDegreesOfCellsReachableByOpponent)
                    + weightings.NumberOfCellsClosestToPlayerFactor
                        * (gameState.NumberOfCellsClosestToYou - gameState.NumberOfCellsClosestToOpponent)
                    + weightings.TotalDegreesOfCellsClosestToPlayerFactor
                        * (gameState.TotalDegreesOfCellsClosestToYou - gameState.TotalDegreesOfCellsClosestToOpponent)
                    + weightings.NumberOfComponentBranchesInTreeFactor
                        * (gameState.NumberOfComponentBranchesInYourTree - gameState.NumberOfComponentBranchesInOpponentsTree)
                    + weightings.SumOfDistancesFromThisPlayerOnClosestCellsFactor
                        * (gameState.SumOfDistancesFromYouOnYourClosestCells - gameState.SumOfDistancesFromOpponentOnOpponentsClosestCells)
                    + weightings.SumOfDistancesFromOtherPlayerOnClosestCellsFactor
                        * (gameState.SumOfDistancesFromOpponentOnYourClosestCells - gameState.SumOfDistancesFromYouOnOpponentsClosestCells);
                searchNode.Evaluation = evaluation;
            }
        }

        protected BaseNegaMaxSolverWithMetricsWeightings()
            : this(new MetricsWeightings(), new MetricsWeightings())
        {
        }

        protected BaseNegaMaxSolverWithMetricsWeightings(MetricsWeightings weightingsInSameCompartment, MetricsWeightings weightingsInSeparateCompartment)
        {
            WeightingsInSameCompartment = weightingsInSameCompartment;
            WeightingsInSeparateCompartment = weightingsInSeparateCompartment;
        }
    }
}
