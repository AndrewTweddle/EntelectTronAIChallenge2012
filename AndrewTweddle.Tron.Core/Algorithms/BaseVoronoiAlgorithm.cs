using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public abstract class BaseVoronoiAlgorithm
    {
        protected abstract void CalculateDistancesFromAPlayer(GameState gameState, PlayerType player, HashSet<CellState> reachableCells);

        public void Perform(GameState gameState, bool calculateDistancesFromOpponent = true)
        {
#if DEBUG
            Stopwatch swatch = Stopwatch.StartNew();
#endif
            gameState.ClearDijkstraProperties();
            HashSet<CellState> reachableCells = new HashSet<CellState>();

            CalculateDistancesFromAPlayer(gameState, PlayerType.You, reachableCells);

            if (calculateDistancesFromOpponent)
            {
                CalculateDistancesFromAPlayer(gameState, PlayerType.Opponent, reachableCells);
                CalculateClosestVertices(gameState, reachableCells);
            }
#if DEBUG
            swatch.Stop();
            Debug.WriteLine(String.Format("Dijkstra with {1} spaces filled took {0} ", swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
#endif
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
                            // gameState.AddFrontierCellForPlayer(cellState, gameState.PlayerToMoveNext);
                        }
                        else
                        {
                            if (distanceFromYou < distanceFromOpponent)
                            {
                                cellState.ClosestPlayer = PlayerType.You;
                                // CheckIfCellIsOnFrontierForPlayer(gameState, cellState, PlayerType.You);
                            }
                            else
                            {
                                cellState.ClosestPlayer = PlayerType.Opponent;
                                // CheckIfCellIsOnFrontierForPlayer(gameState, cellState, PlayerType.Opponent);
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

        private static void CheckIfCellIsOnFrontierForPlayer(GameState gameState, CellState cellState, PlayerType playerType)
        {
            PlayerType otherPlayerType;
            OccupationStatus otherPlayerOccupationStatus;
            int distanceFromPlayer;
            int distanceFromOtherPlayer;

            switch (playerType)
            {
                case PlayerType.You:
                    otherPlayerType = PlayerType.Opponent;
                    otherPlayerOccupationStatus = OccupationStatus.Opponent;
                    distanceFromPlayer = cellState.DistanceFromYou;
                    distanceFromOtherPlayer = cellState.DistanceFromOpponent;
                    break;
                default:  // Assume PlayerType.Opponent
                    otherPlayerType = PlayerType.You;
                    otherPlayerOccupationStatus = OccupationStatus.You;
                    distanceFromPlayer = cellState.DistanceFromOpponent;
                    distanceFromOtherPlayer = cellState.DistanceFromYou;
                    break;
            }

            // It's not on the frontier if it's too far from the other player:
            if (distanceFromPlayer < distanceFromOtherPlayer - 2)
            {
                return;
            }

            bool isOnFrontier = false;
            foreach (CellState adjacentCellState in cellState.GetAdjacentCellStates())
            {
                if (adjacentCellState.ClosestPlayer == otherPlayerType
                    && (adjacentCellState.OccupationStatus == OccupationStatus.Clear || adjacentCellState.OccupationStatus == otherPlayerOccupationStatus))
                {
                    isOnFrontier = true;
                    break;
                }
            }

            if (isOnFrontier)
            {
                gameState.AddFrontierCellForPlayer(cellState, playerType);
            }
        }
    }
}
