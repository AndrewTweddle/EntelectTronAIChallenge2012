using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public class BiconnectedComponent: INotifyPropertyChanged
    {
        private HashSet<CellState> cells = new HashSet<CellState>();
        private HashSet<CellState> cutVertices = new HashSet<CellState>();
        HashSet<BiconnectedComponent> adjacentComponents = new HashSet<BiconnectedComponent>();

        #region Metrics

        private Metrics yourMetricsForComponentOnly = new Metrics();
        private Metrics opponentsMetricsForComponentOnly = new Metrics();
        private Metrics subtreeMetricsForYou;
        private Metrics subtreeMetricsForOpponent;
        private bool isSubTreeVisitedForYou;
        private bool isSubTreeVisitedForOpponent;

        #endregion

        public int ComponentNumber
        {
            get;
            private set;
        }

        #region Properties for metrics

        public Metrics YourMetricsForComponentOnly
        {
            get
            {
                return yourMetricsForComponentOnly;
            }
        }

        public Metrics OpponentsMetricsForComponentOnly
        {
            get
            {
                return opponentsMetricsForComponentOnly;
            }
        }

        public Metrics SubtreeMetricsForYou
        {
            get
            {
                return subtreeMetricsForYou;
            }
        }

        public Metrics SubtreeMetricsForOpponent
        {
            get
            {
                return subtreeMetricsForOpponent;
            }
        }

        public CellState EntryVertexForYou
        {
            get;
            set;
        }

        public CellState ExitVertexForYou
        {
            get;
            set;
        }

        public CellState EntryVertexForOpponent
        {
            get;
            set;
        }

        public CellState ExitVertexForOpponent
        {
            get;
            set;
        }

        public bool IsSubTreeVisitedForYou
        {
            get
            {
                return isSubTreeVisitedForYou;
            }
            set
            {
                isSubTreeVisitedForYou = value;
#if DEBUG
                OnPropertyChanged("IsSubTreeVisitedForYou");
#endif
            }
        }

        public bool IsSubTreeVisitedForOpponent
        {
            get
            {
                return isSubTreeVisitedForOpponent;
            }
            set
            {
                isSubTreeVisitedForOpponent = value;
#if DEBUG
                OnPropertyChanged("IsSubTreeVisitedForOpponent");
#endif
            }
        }

        #endregion

        #region Constructors

        protected BiconnectedComponent()
        {
        }

        public BiconnectedComponent(int componentNumber)
        {
            ComponentNumber = componentNumber;
        }

        #endregion

        public void AddCell(CellState cellState)
        {
            cells.Add(cellState);
        }

        public IEnumerable<CellState> GetCells()
        {
            return cells;
        }

        public IEnumerable<CellState> GetCutVertices()
        {
            return cutVertices;
        }

        public IEnumerable<BiconnectedComponent> GetAdjacentComponents()
        {
            return adjacentComponents;
        }

        public void AddCutVertex(CellState cellState)
        {
            // TODO: Make sure cellState is part of the component
            cutVertices.Add(cellState);
        }

        public void CalculateAdjacentComponents()
        {
            foreach (CellState cutVertex in GetCutVertices())
            {
                foreach (BiconnectedComponent adjacentComponent in cutVertex.GetBiconnectedComponents())
                {
                    if (adjacentComponent != this)
                    {
                        adjacentComponents.Add(adjacentComponent);
                    }
                }
            }
        }

        public void CalculateMetricsOfComponent()
        {
            int numberOfCellsReachableByYou = 0;
            int numberOfCellsReachableByOpponent = 0;
            int totalDegreesOfCellsReachableByYou = 0;
            int totalDegreesOfCellsReachableByOpponent = 0;
            int numberOfCellsClosestToYou = 0;
            int numberOfCellsClosestToOpponent = 0;
            int totalDegreesOfCellsClosestToYou = 0;
            int totalDegreesOfCellsClosestToOpponent = 0;
            int totalSumOfDistancesFromYouOnYourClosestCells = 0;
            int totalSumOfDistancesFromOpponentOnYourClosestCells = 0;
            int totalSumOfDistancesFromYouOnOpponentsClosestCells = 0;
            int totalSumOfDistancesFromOpponentOnOpponentsClosestCells = 0;

            foreach (CellState cellState in GetCells())
            {
                if (!cellState.IsACutVertex)
                {
                    bool isAPole = cellState.Position.IsPole;
                        // Ignore degrees of a pole, as they are typically large, which skews evaluations based on them

                    // Cap distances at 900, otherwise unreachable cells / light cycle's cell is int.MaxValue, which makes all other values meaningless:
                    int distanceFromYou = cellState.DistanceFromYou > 900 ? 900 : cellState.DistanceFromYou;
                    int distanceFromOpponent = cellState.DistanceFromOpponent > 900 ? 900 : cellState.DistanceFromOpponent;

                    // Update your metrics:
                    if (cellState.CompartmentStatus == CompartmentStatus.InYourCompartment || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        numberOfCellsReachableByYou++;
                        if (!isAPole)
                        {
                            totalDegreesOfCellsReachableByYou += cellState.DegreeOfVertex;
                        }
                        if (cellState.ClosestPlayer == PlayerType.You)
                        {
                            numberOfCellsClosestToYou++;
                            if (!isAPole)
                            {
                                totalDegreesOfCellsClosestToYou += cellState.DegreeOfVertex;
                            }
                            totalSumOfDistancesFromYouOnYourClosestCells += distanceFromYou;
                            totalSumOfDistancesFromOpponentOnYourClosestCells += distanceFromOpponent;
                        }
                    }

                    // Update opponents metrics:
                    if (cellState.CompartmentStatus == CompartmentStatus.InOpponentsCompartment || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        numberOfCellsReachableByOpponent++;
                        if (!isAPole)
                        {
                            totalDegreesOfCellsReachableByOpponent += cellState.DegreeOfVertex;
                        }
                        if (cellState.ClosestPlayer == PlayerType.Opponent)
                        {
                            numberOfCellsClosestToOpponent++;
                            if (!isAPole)
                            {
                                totalDegreesOfCellsClosestToOpponent += cellState.DegreeOfVertex;
                            }
                            totalSumOfDistancesFromYouOnOpponentsClosestCells += distanceFromYou;
                            totalSumOfDistancesFromOpponentOnOpponentsClosestCells += distanceFromOpponent;
                        }
                    }
                }
            }

            yourMetricsForComponentOnly.NumberOfCellsReachableByPlayer = numberOfCellsReachableByYou;
            yourMetricsForComponentOnly.TotalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByYou;
            yourMetricsForComponentOnly.NumberOfCellsClosestToPlayer = numberOfCellsClosestToYou;
            yourMetricsForComponentOnly.TotalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToYou;
            yourMetricsForComponentOnly.SumOfDistancesFromThisPlayerOnClosestCells = totalSumOfDistancesFromYouOnYourClosestCells;
            yourMetricsForComponentOnly.SumOfDistancesFromOtherPlayerOnClosestCells = totalSumOfDistancesFromOpponentOnYourClosestCells;

            opponentsMetricsForComponentOnly.NumberOfCellsReachableByPlayer = numberOfCellsReachableByOpponent;
            opponentsMetricsForComponentOnly.TotalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByOpponent;
            opponentsMetricsForComponentOnly.NumberOfCellsClosestToPlayer = numberOfCellsClosestToOpponent;
            opponentsMetricsForComponentOnly.TotalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToOpponent;
            opponentsMetricsForComponentOnly.SumOfDistancesFromThisPlayerOnClosestCells = totalSumOfDistancesFromOpponentOnOpponentsClosestCells;
            opponentsMetricsForComponentOnly.SumOfDistancesFromOtherPlayerOnClosestCells = totalSumOfDistancesFromYouOnOpponentsClosestCells;
        }

        // TODO: Add CompartmentStatus property and a method to calculate it

        public void CalculateSubtreeMetricsForPlayer(PlayerType playerType, MetricsEvaluator evaluator, CellState entryVertex)
        {
            switch (playerType)
            {
                case PlayerType.You:
                    CalculateSubtreeMetricsForYou(evaluator, entryVertex);
                    break;
                case PlayerType.Opponent:
                    CalculateSubtreeMetricsForOpponent(evaluator, entryVertex);
                    break;
            }
        }

        public void CalculateSubtreeMetricsForYou(MetricsEvaluator evaluator, CellState entryVertex)
        {
            EntryVertexForYou = entryVertex;
            CellState bestCutVertex = null;
            double valueOfBestCutVertex = double.NegativeInfinity;
            int branchCount = 0;

            // TODO: Order the cut vertices from most promising to least promising, in case there is a cycle:
            foreach (CellState cutVertex in cutVertices)
            {
                if (cutVertex != entryVertex && cutVertex.OccupationStatus != OccupationStatus.Opponent)
                {
                    if (cutVertex.IsSubTreeVisitedForYou)
                    {
                        System.Diagnostics.Debug.WriteLine("Cut vertex {0} not being visited, as it has been visited previously for You");
                    }
                    else
                    {
                        cutVertex.CalculateSubtreeMetricsForYou(evaluator, this);
                        Metrics cutVertexSubtreeMetrics = cutVertex.SubtreeMetricsForYou;
                        double valueOfCutVertex = evaluator.Evaluate(cutVertexSubtreeMetrics);
                        if (valueOfCutVertex >= valueOfBestCutVertex)
                        {
                            valueOfBestCutVertex = valueOfCutVertex;
                            bestCutVertex = cutVertex;
                        }
                        branchCount += cutVertexSubtreeMetrics.NumberOfComponentBranchesInTree;
                    }
                }
            }

            // If this is a leaf component, then its branchCount is 1:
            if (branchCount == 0)
            {
                branchCount = 1;
            }

            ExitVertexForYou = bestCutVertex;
            if (bestCutVertex == null)
            {
                subtreeMetricsForYou = yourMetricsForComponentOnly.Clone();
            }
            else
            {
                subtreeMetricsForYou = yourMetricsForComponentOnly + bestCutVertex.SubtreeMetricsForYou;
            }
            subtreeMetricsForYou.NumberOfComponentBranchesInTree = branchCount;
            IsSubTreeVisitedForYou = true;
        }

        public void CalculateSubtreeMetricsForOpponent(MetricsEvaluator evaluator, CellState entryVertex)
        {
            EntryVertexForOpponent = entryVertex;
            CellState bestCutVertex = null;
            double valueOfBestCutVertex = double.NegativeInfinity;
            int branchCount = 0;

            // TODO: Order the cut vertices from most promising to least promising, in case there is a cycle:
            foreach (CellState cutVertex in cutVertices)
            {
                if (cutVertex != entryVertex && cutVertex.OccupationStatus != OccupationStatus.You)
                {
                    if (cutVertex.IsSubTreeVisitedForOpponent)
                    {
                        System.Diagnostics.Debug.WriteLine("Cut vertex {0} not being visited, as it has been visited previously for Opponent");
                    }
                    else
                    {
                        cutVertex.CalculateSubtreeMetricsForOpponent(evaluator, this);
                        Metrics cutVertexSubtreeMetrics = cutVertex.SubtreeMetricsForOpponent;
                        double valueOfCutVertex = evaluator.Evaluate(cutVertexSubtreeMetrics);
                        if (valueOfCutVertex >= valueOfBestCutVertex)
                        {
                            valueOfBestCutVertex = valueOfCutVertex;
                            bestCutVertex = cutVertex;
                        }
                        branchCount += cutVertexSubtreeMetrics.NumberOfComponentBranchesInTree;
                    }
                }
            }

            // If this is a leaf component, then its branchCount is 1:
            if (branchCount == 0)
            {
                branchCount = 1;
            }

            ExitVertexForOpponent = bestCutVertex;
            if (bestCutVertex == null)
            {
                subtreeMetricsForOpponent = opponentsMetricsForComponentOnly.Clone();
            }
            else
            {
                subtreeMetricsForOpponent = opponentsMetricsForComponentOnly + bestCutVertex.SubtreeMetricsForOpponent;
            }

            subtreeMetricsForOpponent.NumberOfComponentBranchesInTree = branchCount;
            IsSubTreeVisitedForOpponent = true;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }
    }
}
