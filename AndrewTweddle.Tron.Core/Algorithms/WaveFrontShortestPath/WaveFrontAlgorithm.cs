using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    /// <summary>
    /// An algorithm of my own invention to determine shortest paths from a point to all other connected points.
    /// It uses a "wavefront" with a particular direction (diagonal or vertical, from the poles).
    /// Edges of wavefronts are either shared with adjacent wavefronts (at 90 degrees), 
    /// or a new "eddy" wavefront is created at the edges of the existing wavefronts, 
    /// to "backfill" into areas behind barriers.
    /// </summary>
    public class WaveFrontAlgorithm: BaseVoronoiAlgorithm
    {
        protected override void CalculateDistancesFromAPlayer(GameState gameState, PlayerType player, HashSet<CellState> reachableCells)
        {
            CellState startingCell = null;
            PlayerCalculator calculator = null;
            switch (player)
            {
                case PlayerType.You:
                    gameState.OpponentIsInSameCompartment = false;  // Clear this property, so the algorithm can set it
                    calculator = new YouCalculator();
                    startingCell = gameState.YourCell;
                    break;
                case PlayerType.Opponent:
                    startingCell = gameState.OpponentsCell;
                    calculator = new OpponentCalculator();
                    break;
            }

            /* Set up the initial wave fronts, all for a single point (the player's position): */
            Position startPoint = startingCell.Position;
            LinkedList<WaveFront> currentWaveFronts = new LinkedList<WaveFront>();
            if (startingCell == gameState.NorthPole)
            {
                WaveFront waveFront = new SouthTravellingPolarWaveFront
                {
                    WesternPoint = startPoint,
                    EasternPoint = startPoint
                };
                currentWaveFronts.AddLast(waveFront);
            }
            else
                if (startingCell == gameState.SouthPole)
                {
                    WaveFront waveFront = new NorthTravellingPolarWaveFront
                    {
                        WesternPoint = startPoint,
                        EasternPoint = startPoint
                    };
                    currentWaveFronts.AddLast(waveFront);
                }
                else
                {
                    /* Create 4 wave fronts of a single point each, in each of the diagonal directions: */
                    WaveDirection[] diagonalDirections = { WaveDirection.NW, WaveDirection.NE, WaveDirection.SE, WaveDirection.SW };
                    foreach (WaveDirection direction in diagonalDirections)
                    {
                        WaveFront waveFront = WaveFrontFactory.CreateWaveFront(direction);
                        waveFront.WesternPoint = waveFront.EasternPoint = startPoint;
                        waveFront.IsWesternPointShared = true;
                        waveFront.IsEasternPointShared = true;
                        currentWaveFronts.AddLast(waveFront);
                    }
                }

            /* Iteratively expand the wave fronts until there are none left to expand: */
            int nextDistance = 0;
            LinkedList<WaveFront> nextWaveFronts;

            while (currentWaveFronts.Count > 0)
            {
                nextDistance++;
                nextWaveFronts = new LinkedList<WaveFront>();
                foreach (WaveFront currentWaveFront in currentWaveFronts)
                {
                    WaveFront expandedWaveFront = currentWaveFront.Expand();
                    IEnumerable<WaveFront> newWaveFronts = expandedWaveFront.ContractAndCalculate(gameState, nextDistance, calculator, reachableCells);
                    foreach (WaveFront newWaveFront in newWaveFronts)
                    {
                        nextWaveFronts.AddLast(newWaveFront);
                    }
                }
                currentWaveFronts = nextWaveFronts;
            }

            /* Update aggregate data: */
            switch (player)
            {
                case PlayerType.You:
                    gameState.NumberOfCellsReachableByYou = calculator.NumberOfCellsReachable;
                    gameState.TotalDegreesOfCellsReachableByYou = calculator.TotalDegreesOfCellsReachable;
                    gameState.YourDijkstraStatus = DijkstraStatus.FullyCalculated;
                    gameState.YourUpToDateDijkstraDistance = int.MaxValue;
                    break;

                case PlayerType.Opponent:
                    gameState.NumberOfCellsReachableByOpponent = calculator.NumberOfCellsReachable;
                    gameState.TotalDegreesOfCellsReachableByOpponent = calculator.TotalDegreesOfCellsReachable;
                    gameState.OpponentsDijkstraStatus = DijkstraStatus.FullyCalculated;
                    gameState.OpponentsUpToDateDijkstraDistance = int.MaxValue;
                    break;
            }
        }
    }
}
