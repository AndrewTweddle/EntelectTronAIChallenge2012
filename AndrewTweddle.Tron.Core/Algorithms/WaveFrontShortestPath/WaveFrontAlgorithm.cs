using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public static class WaveFrontAlgorithm
    {
        public static void Perform(GameState gameState, bool calculateDistancesFromOpponent = true)
        {
            Stopwatch swatch = Stopwatch.StartNew();

            gameState.ClearDijkstraProperties();
            HashSet<CellState> reachableCells = new HashSet<CellState>();

            CalculateDistancesFromAPlayer(gameState, PlayerType.You, reachableCells);

            if (calculateDistancesFromOpponent)
            {
                CalculateDistancesFromAPlayer(gameState, PlayerType.Opponent, reachableCells);
                CalculateClosestVertices(gameState, reachableCells);
            }

            swatch.Stop();
            Debug.WriteLine(String.Format("Dijkstra with {1} spaces filled took {0} ", swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
        }

        private static void CalculateDistancesFromAPlayer(GameState gameState, PlayerType player, HashSet<CellState> reachableCells)
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

        private static void CalculateClosestVertices(GameState gameState, HashSet<CellState> reachableCells)
        {
            int numberOfCellsClosestToYou = 0;
            int numberOfCellsClosestToOpponent = 0;
            int totalDegreesOfCellsClosestToYou = 0;
            int totalDegreesOfCellsClosestToOpponent = 0;

            foreach (CellState cellState in reachableCells)
            {
                switch (cellState.CompartmentStatus)
                {
                    case CompartmentStatus.InOpponentsCompartment:
                        cellState.ClosestPlayer = PlayerType.Opponent;
                        break;

                    case CompartmentStatus.InYourCompartment:
                        cellState.ClosestPlayer = PlayerType.You;
                        break;

                    case CompartmentStatus.InSharedCompartment:
                        int distanceFromYou = cellState.DistanceFromYou;
                        int distanceFromOpponent = cellState.DistanceFromOpponent;
                        if (distanceFromYou == distanceFromOpponent)
                        {
                            cellState.ClosestPlayer = gameState.PlayerToMoveNext;
                        }
                        else
                        {
                            if (distanceFromYou < distanceFromOpponent)
                            {
                                cellState.ClosestPlayer = PlayerType.You;
                            }
                            else
                            {
                                cellState.ClosestPlayer = PlayerType.Opponent;
                            }
                        }
                        break;

                    default:
                        throw new ApplicationException(
                            String.Format(
                                "The cell at ({0}, {1}) is not showing in either player's compartment, but it is in the reachable set",
                                cellState.Position.X, cellState.Position.Y));
                }

                if (cellState.OccupationStatus == OccupationStatus.Clear)
                {
                    switch (cellState.ClosestPlayer)
                    {
                        case PlayerType.You:
                            numberOfCellsClosestToYou++;
                            totalDegreesOfCellsClosestToYou += cellState.DegreeOfVertex;
                            break;

                        case PlayerType.Opponent:
                            numberOfCellsClosestToOpponent++;
                            totalDegreesOfCellsClosestToOpponent += cellState.DegreeOfVertex;
                            break;
                    }
                }
            }

#if DEBUG
            if (numberOfCellsClosestToYou == 0 || numberOfCellsClosestToOpponent == 0)
            {
                System.Diagnostics.Debug.WriteLine("Number of closest cells is zero");
            }
#endif

            gameState.NumberOfCellsClosestToYou = numberOfCellsClosestToYou;
            gameState.NumberOfCellsClosestToOpponent = numberOfCellsClosestToOpponent;
            gameState.TotalDegreesOfCellsClosestToYou = totalDegreesOfCellsClosestToYou;
            gameState.TotalDegreesOfCellsClosestToOpponent = totalDegreesOfCellsClosestToOpponent;
        }
    }
}
