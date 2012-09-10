using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public class BiconnectedComponentsAlgorithm
    {
        int count = 0;
        Stack<Edge> edgeStack;

        public void Calculate(GameState gameState)
        {
#if DEBUG
            Stopwatch swatch = Stopwatch.StartNew();
#endif
            count = 0;
            edgeStack = new Stack<Edge>();
            Queue<CellState> cellsToVisit = new Queue<CellState>();

            foreach (CellState cellState in gameState.GetAllCellStates())
            {
                if (cellState.OccupationStatus != OccupationStatus.YourWall && cellState.OccupationStatus != OccupationStatus.OpponentWall)
                {
                    cellState.Visited = false;
                    cellState.ParentCellState = null;
                    cellsToVisit.Enqueue(cellState);
                }
            }
            int cellsToVisitCount = cellsToVisit.Count;
            while (cellsToVisitCount !=0)
            {
                CellState nextCellToVisit = cellsToVisit.Dequeue();
                if (!nextCellToVisit.Visited )
                {
                    Visit(nextCellToVisit);
                }
                cellsToVisitCount--;
            }
#if DEBUG
            swatch.Stop();
            Debug.WriteLine(String.Format("Biconnected components algorithm with {1} spaces filled took {0} ", swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
#endif
        }

        private void Visit(CellState startCellState)
        {
            startCellState.Visited = true;
            count++;
            startCellState.DfsDepth = count;
            startCellState.DfsLow = count;
            foreach (CellState adjacentCellState in startCellState.GetAdjacentCellStates())
            {
                if (adjacentCellState.OccupationStatus != OccupationStatus.YourWall && adjacentCellState.OccupationStatus != OccupationStatus.OpponentWall)
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
                            // Create a biconnected component
                            BiconnectedComponent component = new BiconnectedComponent();
                            startCellState.GameState.AddBiconnectedComponent(component);

                            Edge poppedEdge = null;
                            do
                            {
                                poppedEdge = edgeStack.Pop();
                                component.AddCell(poppedEdge.EndVertex);
                                component.AddCell(poppedEdge.StartVertex);
                                poppedEdge.StartVertex.AddBiconnectedComponent(component);
                                poppedEdge.EndVertex.AddBiconnectedComponent(component);
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
