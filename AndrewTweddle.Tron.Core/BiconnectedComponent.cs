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

        private int numberOfCellsReachableByYou;
        private int numberOfCellsReachableByOpponent;
        private int totalDegreesOfCellsReachableByYou;
        private int totalDegreesOfCellsReachableByOpponent;
        private int numberOfCellsClosestToYou;
        private int numberOfCellsClosestToOpponent;
        private int totalDegreesOfCellsClosestToYou;
        private int totalDegreesOfCellsClosestToOpponent;

        #endregion

        public int ComponentNumber
        {
            get;
            private set;
        }

        #region Properties for metrics

        public int NumberOfCellsReachableByYou
        {
            get
            {
                return numberOfCellsReachableByYou;
            }
            set
            {
                numberOfCellsReachableByYou = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsReachableByYou");
#endif
            }
        }

        public int NumberOfCellsReachableByOpponent
        {
            get
            {
                return numberOfCellsReachableByOpponent;
            }
            set
            {
                numberOfCellsReachableByOpponent = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsReachableByOpponent");
#endif
            }
        }

        public int TotalDegreesOfCellsReachableByYou
        {
            get
            {
                return totalDegreesOfCellsReachableByYou;
            }
            set
            {
                totalDegreesOfCellsReachableByYou = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsReachableByYou");
#endif
            }
        }

        public int TotalDegreesOfCellsReachableByOpponent
        {
            get
            {
                return totalDegreesOfCellsReachableByOpponent;
            }
            set
            {
                totalDegreesOfCellsReachableByOpponent = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsReachableByOpponent");
#endif
            }
        }

        public int NumberOfCellsClosestToYou
        {
            get
            {
                return numberOfCellsClosestToYou;
            }
            set
            {
                numberOfCellsClosestToYou = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsClosestToYou");
#endif
            }
        }

        public int NumberOfCellsClosestToOpponent
        {
            get
            {
                return numberOfCellsClosestToOpponent;
            }
            set
            {
                numberOfCellsClosestToOpponent = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsClosestToOpponent");
#endif
            }
        }

        public int TotalDegreesOfCellsClosestToYou
        {
            get
            {
                return totalDegreesOfCellsClosestToYou;
            }
            set
            {
                totalDegreesOfCellsClosestToYou = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsClosestToYou");
#endif
            }
        }

        public int TotalDegreesOfCellsClosestToOpponent
        {
            get
            {
                return totalDegreesOfCellsClosestToOpponent;
            }
            set
            {
                totalDegreesOfCellsClosestToOpponent = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsClosestToOpponent");
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
                    if (cellState.CompartmentStatus == CompartmentStatus.InYourCompartment || cellState.CompartmentStatus == CompartmentStatus.InSharedCompartment)
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

            NumberOfCellsReachableByYou = numberOfCellsReachableByYou;
            NumberOfCellsReachableByOpponent = numberOfCellsReachableByOpponent;
            TotalDegreesOfCellsReachableByYou = totalDegreesOfCellsReachableByYou;
            TotalDegreesOfCellsReachableByOpponent = totalDegreesOfCellsReachableByOpponent;
            NumberOfCellsClosestToYou = numberOfCellsClosestToYou;
            NumberOfCellsClosestToOpponent = numberOfCellsClosestToOpponent;
            TotalDegreesOfCellsClosestToYou = totalDegreesOfCellsClosestToYou;
            TotalDegreesOfCellsClosestToOpponent = totalDegreesOfCellsClosestToOpponent;
        }

        // TODO: Add CompartmentStatus property and a method to calculate it


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
