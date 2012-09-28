using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public class ShortestPathAlgorithmUsingHashSets: BaseVoronoiAlgorithm
    {
        protected override void CalculateDistancesFromAPlayer(GameState gameState, PlayerType player, HashSet<CellState> reachableCells)
        {
            int nextDistance = 0;
            int numberOfCellsReachable = 0;
            int totalDegreesOfCellsReachable = 0;
            HashSet<CellState> cellsToExpand = new HashSet<CellState>();

            DijkstraStatus dijkstraStatus = DijkstraStatus.NotCalculated;
            bool isPartiallyCalculated = false;
            int upToDateDijkstraDistance = 0;
            CellState playersCell;

            CompartmentStatus compartmentStatus;
            GetDistanceDelegate getDistance = null;
            SetDistanceDelegate setDistance = null;

            switch (player)
            {
                case PlayerType.You:
                    compartmentStatus = CompartmentStatus.InYourCompartment;
                    getDistance = GetDistanceFromYou;
                    setDistance = SetDistanceFromYou;
                    dijkstraStatus = gameState.YourDijkstraStatus;
                    upToDateDijkstraDistance = gameState.YourUpToDateDijkstraDistance;
                    playersCell = gameState.YourCell;
                    break;
                case PlayerType.Opponent:
                    compartmentStatus = CompartmentStatus.InOpponentsCompartment;
                    getDistance = GetDistanceFromOpponent;
                    setDistance = SetDistanceFromOpponent;
                    dijkstraStatus = gameState.OpponentsDijkstraStatus;
                    upToDateDijkstraDistance = gameState.OpponentsUpToDateDijkstraDistance;
                    playersCell = gameState.OpponentsCell;
                    break;
                default:
                    throw new ApplicationException("The player must be specified when calculating distances");
            }

            if (dijkstraStatus != DijkstraStatus.FullyCalculated)
            {
                if (player == PlayerType.You)
                {
                    gameState.OpponentIsInSameCompartment = false;
                }

                isPartiallyCalculated = (dijkstraStatus == DijkstraStatus.PartiallyCalculated && upToDateDijkstraDistance > 0);

                if (isPartiallyCalculated)
                {
                    nextDistance = upToDateDijkstraDistance;
                    foreach (CellState cellState in gameState.GetAllCellStates())
                    {
                        int distance = getDistance(cellState);
                        if ((cellState.OccupationStatus == OccupationStatus.Clear)
                            && (cellState.CompartmentStatus == compartmentStatus || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                            && (distance <= upToDateDijkstraDistance))
                        {
                            if (reachableCells != null)
                            {
                                reachableCells.Add(cellState);
                            }
                            numberOfCellsReachable++;
                            totalDegreesOfCellsReachable += cellState.DegreeOfVertex;
                            if (distance == upToDateDijkstraDistance)
                            {
                                cellsToExpand.Add(cellState);
                            }
                        }
                        else
                        {
                            cellState.ClearDijkstraStateForPlayer(player);
                        }
                    }
                }
                else
                {
                    gameState.ClearDijkstraPropertiesForPlayer(player);
                    cellsToExpand.Add(playersCell);
                }

                while (cellsToExpand.Any())
                {
                    nextDistance++;
                    HashSet<CellState> nextLevelOfCells = new HashSet<CellState>();
                    foreach (CellState sourceCell in cellsToExpand)
                    {
                        CellState[] adjacentCells = sourceCell.GetAdjacentCellStates();
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
                                    int existingDistance = getDistance(adjacentCell);
                                    if (nextDistance < existingDistance)
                                    {
                                        setDistance(adjacentCell, nextDistance);

                                        // HashSets automatically filter out duplicates, so no need to check:
                                        nextLevelOfCells.Add(adjacentCell);
                                        if (reachableCells != null)
                                        {
                                            reachableCells.Add(adjacentCell);
                                        }
                                    }
                                    break;
                            }
                        }
                        numberOfCellsReachable++;
                        totalDegreesOfCellsReachable += sourceCell.DegreeOfVertex;
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

                switch (player)
                {
                    case PlayerType.You:
                        gameState.YourDijkstraStatus = DijkstraStatus.FullyCalculated;
                        gameState.YourUpToDateDijkstraDistance = int.MaxValue;
                        break;
                    case PlayerType.Opponent:
                        gameState.OpponentsDijkstraStatus = DijkstraStatus.FullyCalculated;
                        gameState.OpponentsUpToDateDijkstraDistance = int.MaxValue;
                        break;
                }
            }
            else
            {
                // Fix ClosestPlayer:
                foreach (CellState cellState in gameState.GetAllCellStates())
                {
                    if (cellState.ClosestPlayer == PlayerType.Unknown)
                    {
                        switch (cellState.CompartmentStatus)
                        {
                            case CompartmentStatus.InYourCompartment:
                                cellState.ClosestPlayer = PlayerType.You;
                                break;
                            case CompartmentStatus.InOpponentsCompartment:
                                cellState.ClosestPlayer = PlayerType.Opponent;
                                break;
                            case CompartmentStatus.InSharedCompartment:
                                if (cellState.DistanceFromYou > cellState.DistanceFromOpponent)
                                {
                                    cellState.ClosestPlayer = PlayerType.You;
                                }
                                else
                                    if (cellState.DistanceFromOpponent > cellState.DistanceFromYou)
                                    {
                                        cellState.ClosestPlayer = PlayerType.Opponent;
                                    }
                                    else
                                    {
                                        cellState.ClosestPlayer = gameState.PlayerToMoveNext;
                                    }
                                break;
                        }
                    }
                }

                gameState.YourCell.ClosestPlayer = PlayerType.You;
                gameState.OpponentsCell.ClosestPlayer = PlayerType.Opponent;
            }
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
