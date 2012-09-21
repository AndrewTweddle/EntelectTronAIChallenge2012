using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    /// <summary>
    /// This is the same as the biconnected components algorithm, except it only works on a subset of the cells - those closest to a particular player
    /// </summary>
    public class BiconnectedChambersAlgorithm
    {
        int count;
        int nextChamberNumber;
        Stack<Edge> edgeStack;
        OccupationStatus playerOccupationStatus;
        PlayerType player;
        GameState gameState;

        public void Calculate(GameState gameState)
        {
#if DEBUG
            Stopwatch swatch = Stopwatch.StartNew();
#endif
            this.gameState = gameState;
            CalculateChambersForPlayer(PlayerType.You);
            CalculateChambersForPlayer(PlayerType.Opponent);
#if DEBUG
            swatch.Stop();
            Debug.WriteLine(String.Format("Chambers algorithm with {1} spaces filled took {0} ",
                swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
#endif

        }

        public void CalculateChambersForPlayer(PlayerType player)
        {
            this.player = player;
            count = 0;
            nextChamberNumber = 1;
            switch (player)
            {
                case PlayerType.You:
                    playerOccupationStatus = OccupationStatus.You;
                    break;
                default:
                    playerOccupationStatus = OccupationStatus.Opponent;
                    break;
            }

            gameState.ClearChamberPropertiesForPlayer(player);

            edgeStack = new Stack<Edge>();
            Queue<CellState> cellsToVisit = new Queue<CellState>();

            foreach (CellState cellState in gameState.GetAllCellStates())
            {
                if (cellState.ClosestPlayer == player
                    && (cellState.OccupationStatus == OccupationStatus.Clear || cellState.OccupationStatus == playerOccupationStatus))
                {
                    cellState.ClearChamberPropertiesForPlayer(player);
                    cellsToVisit.Enqueue(cellState);
                }
            }

            int cellsToVisitCount = cellsToVisit.Count;
            while (cellsToVisitCount != 0)
            {
                CellState nextCellToVisit = cellsToVisit.Dequeue();
                if (!nextCellToVisit.Visited)
                {
                    Visit(nextCellToVisit);
                }
                cellsToVisitCount--;
            }
        }

        private void Visit(CellState startCellState)
        {
            startCellState.Visited = true;
            count++;
            startCellState.DfsDepth = count;
            startCellState.DfsLow = count;
            foreach (CellState adjacentCellState in startCellState.GetAdjacentCellStates())
            {
                if (adjacentCellState.ClosestPlayer == player &&
                    (adjacentCellState.OccupationStatus == OccupationStatus.Clear || adjacentCellState.OccupationStatus == playerOccupationStatus))
                {
                    if (!adjacentCellState.Visited)
                    {
                        Edge newEdge = new Edge
                        {
                            StartVertex = startCellState,
                            EndVertex = adjacentCellState
                        };
                        edgeStack.Push(newEdge);
                        adjacentCellState.ParentCellState = startCellState;
                        Visit(adjacentCellState);
                        if (adjacentCellState.DfsLow >= startCellState.DfsDepth)
                        {
                            // Create a chamber
                            Chamber chamber = new Chamber(nextChamberNumber);
                            nextChamberNumber++;
                            startCellState.GameState.AddChamber(chamber, player);

                            Edge poppedEdge = null;
                            do
                            {
                                poppedEdge = edgeStack.Pop();
                                chamber.AddCell(poppedEdge.StartVertex);
                                chamber.AddCell(poppedEdge.EndVertex);
                                poppedEdge.StartVertex.AddChamber(chamber, player);
                                poppedEdge.EndVertex.AddChamber(chamber, player);
                            }
                            while (poppedEdge != newEdge);
                        }
                        if (adjacentCellState.DfsLow < startCellState.DfsLow)
                        {
                            startCellState.DfsLow = adjacentCellState.DfsLow;
                        }
                    }
                    else
                        if (!(adjacentCellState.ParentCellState == startCellState) && (adjacentCellState.DfsDepth < startCellState.DfsDepth))
                        {
                            // The link from startCellState to adjacentCellState is a back edge to an ancestor of startCellState:
                            Edge backEdge = new Edge
                            {
                                StartVertex = startCellState,
                                EndVertex = adjacentCellState
                            };
                            edgeStack.Push(backEdge);
                            if (adjacentCellState.DfsDepth < startCellState.DfsLow)
                            {
                                startCellState.DfsLow = adjacentCellState.DfsDepth;
                            }
                        }
                }
            }
        }
    }
}
