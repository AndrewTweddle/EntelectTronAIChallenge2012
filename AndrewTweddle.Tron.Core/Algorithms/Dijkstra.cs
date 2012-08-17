using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public static class Dijkstra
    {
        public static void Perform(GameState gameState, bool calculateDistancesFromOpponent = true)
        {
            Stopwatch swatch = Stopwatch.StartNew();

            gameState.ClearDijkstraProperties();

            TimeSpan elapsed = swatch.Elapsed;
            Debug.WriteLine(String.Format("Clearing Dijkstra properties took {0}", elapsed));

            HashSet<CellState> reachableCells = new HashSet<CellState>();

            CalculateDistancesFromAPlayer(gameState, PlayerType.You, reachableCells);
            if (calculateDistancesFromOpponent)
            {
                CalculateDistancesFromAPlayer(gameState, PlayerType.Opponent, reachableCells);
                CalculateClosestVertices(gameState, reachableCells);
            }

            swatch.Stop();
            Debug.WriteLine(String.Format( "Dijkstra with {1} spaces filled took {0} ", swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
        }

        private static void CalculateDistancesFromAPlayer(GameState gameState, PlayerType player, HashSet<CellState> reachableCells)
        {
            HashSet<CellState> cellsToExpand = new HashSet<CellState>();

            int numberOfCellsReachable = 0;
            int totalDegreesOfCellsReachable = 0;

            CompartmentStatus compartmentStatus;
            GetDistanceDelegate getDistance = null;
            SetDistanceDelegate setDistance = null;

            switch (player)
            {
                case PlayerType.You:
                    compartmentStatus = CompartmentStatus.InYourCompartment;
                    getDistance = GetDistanceFromYou;
                    setDistance = SetDistanceFromYou;
                    cellsToExpand.Add(gameState.YourCell);
                    break;
                case PlayerType.Opponent:
                    compartmentStatus = CompartmentStatus.InOpponentsCompartment;
                    getDistance = GetDistanceFromOpponent;
                    setDistance = SetDistanceFromOpponent;
                    cellsToExpand.Add(gameState.OpponentsCell);
                    break;
                default:
                    throw new ApplicationException("The player must be specified when calculating distances");
            }

            int nextDistance = 0;

            while (cellsToExpand.Any())
            {
                nextDistance++;
                HashSet<CellState> nextLevelOfCells = new HashSet<CellState>();
                foreach (CellState sourceCell in cellsToExpand)
                {
                    int degreeOfVertex = 0;
                    IEnumerable<CellState> adjacentCells = sourceCell.GetAdjacentCellStates();
                    foreach (CellState adjacentCell in adjacentCells)
                    {
                        switch (adjacentCell.OccupationStatus)
                        {
                            case OccupationStatus.Opponent:
                                if (player == PlayerType.You)
                                {
                                    gameState.OpponentIsInSameCompartment = true;
                                }
                                break;
                            case OccupationStatus.Clear:
                                adjacentCell.CompartmentStatus |= compartmentStatus;
                                degreeOfVertex++;
                                int existingDistance = getDistance(adjacentCell);
                                if (nextDistance < existingDistance)
                                {
                                    setDistance(adjacentCell, nextDistance);

                                    // HashSets automatically filter out duplicates, so no need to check:
                                    nextLevelOfCells.Add(adjacentCell);
                                    reachableCells.Add(adjacentCell);
                                }
                                break;
                        }
                    }
                    sourceCell.DegreeOfVertex = degreeOfVertex;
                    numberOfCellsReachable++;
                    totalDegreesOfCellsReachable += degreeOfVertex;
                }
                cellsToExpand = nextLevelOfCells;
            }

            switch (player)
            {
                case PlayerType.You:
                    gameState.NumberOfCellsReachableByYou = numberOfCellsReachable;
                    gameState.TotalDegreesOfCellsReachableByYou = totalDegreesOfCellsReachable;
                    break;

                case PlayerType.Opponent:
                    gameState.NumberOfCellsReachableByOpponent = numberOfCellsReachable;
                    gameState.TotalDegreesOfCellsReachableByOpponent = totalDegreesOfCellsReachable;
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
                        cellState.ClosestPlayer = PlayerType.Opponent;
                        numberOfCellsClosestToOpponent++;
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

            gameState.NumberOfCellsClosestToYou = numberOfCellsClosestToYou;
            gameState.NumberOfCellsClosestToOpponent = numberOfCellsClosestToOpponent;
            gameState.TotalDegreesOfCellsClosestToYou = totalDegreesOfCellsClosestToYou;
            gameState.TotalDegreesOfCellsClosestToOpponent = totalDegreesOfCellsClosestToOpponent;
        }

        private delegate int GetDistanceDelegate(CellState cellState);
        private delegate void SetDistanceDelegate(CellState cellState, int distance);

        private static int GetDistanceFromYou(CellState cellState)
        {
            return cellState.DistanceFromYou;
        }

        private static int GetDistanceFromOpponent(CellState cellState)
        {
            return cellState.DistanceFromOpponent;
        }

        private static void SetDistanceFromYou(CellState cellState, int distance)
        {
            cellState.DistanceFromYou = distance;
        }

        private static void SetDistanceFromOpponent(CellState cellState, int distance)
        {
            cellState.DistanceFromOpponent = distance;
        }
    }
}
