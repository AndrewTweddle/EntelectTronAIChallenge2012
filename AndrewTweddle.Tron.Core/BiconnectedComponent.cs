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

            foreach (CellState cellState in GetCells())
            {
                if (!cellState.IsACutVertex)
                {
                    // Update your metrics:
                    if (cellState.CompartmentStatus == CompartmentStatus.InYourCompartment || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        numberOfCellsReachableByYou++;
                        totalDegreesOfCellsReachableByYou += cellState.DegreeOfVertex;
                        if (cellState.ClosestPlayer == PlayerType.You)
                        {
                            numberOfCellsClosestToYou++;
                            totalDegreesOfCellsClosestToYou += cellState.DegreeOfVertex;
                        }
                    }

                    // Update opponents metrics:
                    if (cellState.CompartmentStatus == CompartmentStatus.InOpponentsCompartment || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        numberOfCellsReachableByOpponent++;
                        totalDegreesOfCellsReachableByOpponent += cellState.DegreeOfVertex;
                        if (cellState.ClosestPlayer == PlayerType.Opponent)
                        {
                            numberOfCellsClosestToOpponent++;
                            totalDegreesOfCellsClosestToOpponent += cellState.DegreeOfVertex;
                        }
                    }
                }
            }

            yourMetricsForComponentOnly.NumberOfCellsReachableByPlayer = numberOfCellsReachableByYou;
            yourMetricsForComponentOnly.TotalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByYou;
            yourMetricsForComponentOnly.NumberOfCellsClosestToPlayer = numberOfCellsClosestToYou;
            yourMetricsForComponentOnly.TotalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToYou;

            opponentsMetricsForComponentOnly.NumberOfCellsReachableByPlayer = numberOfCellsReachableByOpponent;
            opponentsMetricsForComponentOnly.TotalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByOpponent;
            opponentsMetricsForComponentOnly.NumberOfCellsClosestToPlayer = numberOfCellsClosestToOpponent;
            opponentsMetricsForComponentOnly.TotalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToOpponent;
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

            foreach (CellState cutVertex in cutVertices)
            {
                if (cutVertex != entryVertex && cutVertex.OccupationStatus != OccupationStatus.Opponent)
                {
                    cutVertex.CalculateSubtreeMetricsForYou(evaluator, this);
                    Metrics cutVertexSubtreeMetrics = cutVertex.SubtreeMetricsForYou;
                    double valueOfCutVertex = evaluator.Evaluate(cutVertexSubtreeMetrics);
                    if (valueOfCutVertex >= valueOfBestCutVertex)
                    {
                        valueOfBestCutVertex = valueOfCutVertex;
                        bestCutVertex = cutVertex;
                    }
                }
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
        }

        public void CalculateSubtreeMetricsForOpponent(MetricsEvaluator evaluator, CellState entryVertex)
        {
            EntryVertexForOpponent = entryVertex;
            CellState bestCutVertex = null;
            double valueOfBestCutVertex = double.NegativeInfinity;

            foreach (CellState cutVertex in cutVertices)
            {
                if (cutVertex != entryVertex && cutVertex.OccupationStatus != OccupationStatus.You)
                {
                    cutVertex.CalculateSubtreeMetricsForOpponent(evaluator, this);
                    Metrics cutVertexSubtreeMetrics = cutVertex.SubtreeMetricsForOpponent;
                    double valueOfCutVertex = evaluator.Evaluate(cutVertexSubtreeMetrics);
                    if (valueOfCutVertex >= valueOfBestCutVertex)
                    {
                        valueOfBestCutVertex = valueOfCutVertex;
                        bestCutVertex = cutVertex;
                    }
                }
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
