using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public class BiconnectedComponentsAlgorithm
    {
        int count;
        int nextComponentNumber;
        Stack<Edge> edgeStack;

        public void Calculate(GameState gameState, MetricsEvaluator evaluator)
        {
#if DEBUG
            Stopwatch swatch = Stopwatch.StartNew();
#endif
            count = 0;
            nextComponentNumber = 1;

            edgeStack = new Stack<Edge>();
            Queue<CellState> cellsToVisit = new Queue<CellState>();

            gameState.ClearBiconnectedComponentProperties();

            foreach (CellState cellState in gameState.GetAllCellStates())
            {
                if (cellState.OccupationStatus != OccupationStatus.YourWall && cellState.OccupationStatus != OccupationStatus.OpponentWall)
                {
                    cellState.ClearBiconnectedComponentProperties();
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

            PerformPostProcessingOnBiconnectedComponents(gameState, evaluator);
#if DEBUG
            swatch.Stop();
            Debug.WriteLine(String.Format("Biconnected components algorithm with {1} spaces filled took {0} ", swatch.Elapsed, gameState.OpponentsWallLength + gameState.YourWallLength + 2));
#endif

            // TODO: Move to a more appropriate place later...
            BiconnectedChambersAlgorithm bcChambersAlg = new BiconnectedChambersAlgorithm();
            bcChambersAlg.Calculate(gameState);
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
                            BiconnectedComponent component = new BiconnectedComponent(nextComponentNumber);
                            nextComponentNumber++;
                            startCellState.GameState.AddBiconnectedComponent(component);

                            Edge poppedEdge = null;
                            do
                            {
                                poppedEdge = edgeStack.Pop();
                                component.AddCell(poppedEdge.StartVertex);
                                component.AddCell(poppedEdge.EndVertex);
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

        private void PerformPostProcessingOnBiconnectedComponents(GameState gameState, MetricsEvaluator evaluator)
        {
            // Calculate the components adjacent to each component as well as the statistics of each component:
            foreach (BiconnectedComponent component in gameState.GetBiconnectedComponents())
            {
                component.CalculateAdjacentComponents();
                component.CalculateMetricsOfComponent();
            }

            // Calculate the metrics based on a tree of components reachable by you:
            CalculateYourOverallMetrics(gameState, evaluator);
            CalculateOpponentsOverallMetrics(gameState, evaluator);
        }

        private void CalculateYourOverallMetrics(GameState gameState, MetricsEvaluator evaluator)
        {
            Metrics overallMetrics;
            if (gameState.YourCell.IsACutVertex)
            {
                gameState.YourCell.CalculateSubtreeMetricsForYou(evaluator, null /* No entry component */);
                overallMetrics = gameState.YourCell.SubtreeMetricsForYou;
            }
            else
            {
                BiconnectedComponent playersComponent = gameState.YourCell.GetBiconnectedComponents().FirstOrDefault();
                if (playersComponent == null)
                {
                    // The player has no more moves left - there are no edges leading from the cell, and hence no components to be part of!
                    overallMetrics = Metrics.Zero;
                }
                else
                {
                    playersComponent.CalculateSubtreeMetricsForYou(evaluator, gameState.YourCell);
                    overallMetrics = playersComponent.SubtreeMetricsForYou;
                }
            }

            gameState.NumberOfCellsClosestToYou = overallMetrics.NumberOfCellsClosestToPlayer;
            gameState.NumberOfCellsReachableByYou = overallMetrics.NumberOfCellsReachableByPlayer;
            gameState.TotalDegreesOfCellsClosestToYou = overallMetrics.TotalDegreesOfCellsClosestToPlayer;
            gameState.TotalDegreesOfCellsReachableByYou = overallMetrics.TotalDegreesOfCellsReachableByPlayer;
            gameState.SumOfDistancesFromYouOnYourClosestCells = overallMetrics.SumOfDistancesFromThisPlayerOnClosestCells;
            gameState.SumOfDistancesFromOpponentOnYourClosestCells = overallMetrics.SumOfDistancesFromOtherPlayerOnClosestCells;
            gameState.NumberOfComponentBranchesInYourTree = overallMetrics.NumberOfComponentBranchesInTree;
        }

        private void CalculateOpponentsOverallMetrics(GameState gameState, MetricsEvaluator evaluator)
        {
            Metrics overallMetrics;
            if (gameState.OpponentsCell.IsACutVertex)
            {
                gameState.OpponentsCell.CalculateSubtreeMetricsForOpponent(evaluator, null /* No entry component */);
                overallMetrics = gameState.OpponentsCell.SubtreeMetricsForOpponent;
            }
            else
            {
                BiconnectedComponent playersComponent = gameState.OpponentsCell.GetBiconnectedComponents().FirstOrDefault();
                if (playersComponent == null)
                {
                    // The player has no more moves left - there are no edges leading from the cell, and hence no components to be part of!
                    overallMetrics = Metrics.Zero;
                }
                else
                {
                    playersComponent.CalculateSubtreeMetricsForOpponent(evaluator, gameState.OpponentsCell);
                    overallMetrics = playersComponent.SubtreeMetricsForOpponent;
                }
            }

            gameState.NumberOfCellsClosestToOpponent = overallMetrics.NumberOfCellsClosestToPlayer;
            gameState.NumberOfCellsReachableByOpponent = overallMetrics.NumberOfCellsReachableByPlayer;
            gameState.TotalDegreesOfCellsClosestToOpponent = overallMetrics.TotalDegreesOfCellsClosestToPlayer;
            gameState.TotalDegreesOfCellsReachableByOpponent = overallMetrics.TotalDegreesOfCellsReachableByPlayer;
            gameState.SumOfDistancesFromYouOnOpponentsClosestCells = overallMetrics.SumOfDistancesFromOtherPlayerOnClosestCells;
            gameState.SumOfDistancesFromOpponentOnOpponentsClosestCells = overallMetrics.SumOfDistancesFromThisPlayerOnClosestCells;
            gameState.NumberOfComponentBranchesInOpponentsTree = overallMetrics.NumberOfComponentBranchesInTree;
        }
    }
}
