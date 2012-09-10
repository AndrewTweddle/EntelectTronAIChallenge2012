using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace AndrewTweddle.Tron.Core
{
    [Serializable]
    public class CellState: INotifyPropertyChanged
    {
        private GameState gameState;
        private OccupationStatus occupationStatus;
        private int moveNumber;
        private Position position;
        private int distanceFromYou;
        private int distanceFromOpponent;
        private PlayerType closestPlayer;
        private int degreeOfVertex;
        private CompartmentStatus compartmentStatus;

        #region Fields for biconnected components algorithm:

        [NonSerialized]
        private bool visited;
        [NonSerialized]
        private int dfsDepth;
        [NonSerialized]
        private int dfsLow;
        [NonSerialized]
        private CellState parentCellState;
        [NonSerialized]
        private HashSet<BiconnectedComponent> biconnectedComponents;
        [NonSerialized]
        private string biconnectedComponentsListing;

        #endregion

        public GameState GameState
        {
            get
            {
                return gameState;
            }
            private set
            {
                gameState = value;
#if DEBUG
                OnPropertyChanged("GameState");
#endif
            }
        }

        private CellState()
        {
        }

        public CellState(GameState gameState, int x, int y)
        {
            GameState = gameState;
            Position = new Position(x, y);
            DegreeOfVertex = Position.GetInitialDegreeOfVertex();
        }

        public Position Position 
        {
            get
            {
                return position;
            }
            private set
            {
                position = value;
#if DEBUG
                OnPropertyChanged("Position");
#endif
            }
        }
        
        public int MoveNumber 
        {
            get
            {
                return moveNumber;
            }
            internal set
            {
                moveNumber = value;
#if DEBUG
                OnPropertyChanged("MoveNumber");
#endif
            }
        }

        public OccupationStatus OccupationStatus
        { 
            get
            {
                return occupationStatus;
            }
            set
            {
                if (value != OccupationStatus)
                {
                    bool wasFilled = (occupationStatus == OccupationStatus.OpponentWall || occupationStatus == OccupationStatus.YourWall);
                    bool isBeingFilled = (value == OccupationStatus.OpponentWall || value == OccupationStatus.YourWall);
                    occupationStatus = value;
#if DEBUG
                    OnPropertyChanged("OccupationStatus");
#endif

                    /* Does degree of adjacent vertices need to be re-calculated: */
                    if (wasFilled != isBeingFilled)
                    {
                        /* Update degree of vertex and its adjacent vertices: */
                        RecalculateDegree();
                        foreach (CellState adjacentCellState in GetAdjacentCellStates())
                        {
                            adjacentCellState.RecalculateDegree();
                        }
                    }
                }
            }
        }

        public void RecalculateDegree()
        {
            switch (OccupationStatus)
            {
                case OccupationStatus.Clear:
                case OccupationStatus.You:
                case OccupationStatus.Opponent:
                    int degree = 0;
                    foreach (CellState adjacentCellState in GetAdjacentCellStates())
                    {
                        switch (adjacentCellState.OccupationStatus)
                        {
                            case OccupationStatus.Clear:
                            case OccupationStatus.Opponent:
                            case OccupationStatus.You:
                                degree++;
                                break;
                        }
                    }
                    DegreeOfVertex = degree;
                    break;
                default:  // The cell is filled, so it's now connected to nothing...
                    DegreeOfVertex = 0;
                    break;
            }
        }

        #region Dijkstra algorithm

        public int DistanceFromYou 
        {
            get
            {
                return distanceFromYou;
            }
            set
            {
                distanceFromYou = value;
#if DEBUG
                OnPropertyChanged("DistanceFromYou");
#endif
            }
        }

        public int DistanceFromOpponent 
        {
            get
            {
                return distanceFromOpponent;
            }
            set
            {
                distanceFromOpponent = value;
#if DEBUG
                OnPropertyChanged("DistanceFromOpponent");
#endif
            }
        }

        public PlayerType ClosestPlayer 
        {
            get
            {
                return closestPlayer;
            }
            set
            {
                closestPlayer = value;
#if DEBUG
                OnPropertyChanged("ClosestPlayer");
#endif
            }
        }

        public int DegreeOfVertex 
        {
            get
            {
                return degreeOfVertex;
            }
            set
            {
                degreeOfVertex = value;
#if DEBUG
                OnPropertyChanged("DegreeOfVertex");
#endif
            }
        }

        public CompartmentStatus CompartmentStatus 
        {
            get
            {
                return compartmentStatus;
            }
            set
            {
                compartmentStatus = value;
#if DEBUG
                OnPropertyChanged("CompartmentStatus");
#endif
            }
        }

        #endregion

        #region Biconnected Components Algorithm

        public bool Visited
        {
            get
            {
                return visited;
            }
            set
            {
                visited = value;
#if DEBUG
                OnPropertyChanged("Visited");
#endif
            }
        }

        public int DfsDepth
        {
            get
            {
                return dfsDepth;
            }
            set
            {
                dfsDepth = value;
#if DEBUG
                OnPropertyChanged("DfsDepth");
#endif
            }
        }

        public int DfsLow
        {
            get
            {
                return dfsLow;
            }
            set
            {
                dfsLow = value;
#if DEBUG
                OnPropertyChanged("DfsLow");
#endif
            }
        }

        public CellState ParentCellState
        {
            get
            {
                return parentCellState;
            }
            set
            {
                parentCellState = value;
#if DEBUG
                OnPropertyChanged("ParentCellState");
#endif
            }
        }

        public string BiconnectedComponentsListing
        {
            get
            {
                if (biconnectedComponentsListing == null)
                {
                    BiconnectedComponentsListing = CalculateBiconnectedComponentsListing();
                }
                return biconnectedComponentsListing;
            }
            set
            {
                biconnectedComponentsListing = value;
#if DEBUG
                OnPropertyChanged("BiconnectedComponentsListing");
#endif
            }
        }

        /// <summary>
        /// A cut vertex belongs to multiple biconnected components
        /// </summary>
        public bool IsACutVertex
        {
            get
            {
                return (biconnectedComponents != null && biconnectedComponents.Count > 1);
            }
        }

        public void AddBiconnectedComponent(BiconnectedComponent component)
        {
            if (biconnectedComponents == null)
            {
                biconnectedComponents = new HashSet<BiconnectedComponent>();
            }
            int countBefore = biconnectedComponents.Count;
            biconnectedComponents.Add(component);
            int countAfter = biconnectedComponents.Count;

            if (countAfter != countBefore)
            {
                if (countBefore == 1)
                {
                    // It has just become a cut vertex. Update both components accordingly:
                    foreach (BiconnectedComponent existingComponent in biconnectedComponents)
                    {
                        if (existingComponent != component)
                        {
                            existingComponent.AddCutVertex(this);
                        }
                    }
                }
                else
                    if (countBefore > 1)
                    {
                        // It was already a cut vertex. So just add as a cut vertex to the new component:
                        component.AddCutVertex(this);
                    }

                // If in use, update the string listing of the components:
                if (biconnectedComponentsListing != null)
                {
                    BiconnectedComponentsListing = CalculateBiconnectedComponentsListing();
                }
            }
        }

        public IEnumerable<BiconnectedComponent> GetBiconnectedComponents()
        {
            if (biconnectedComponents == null)
            {
                biconnectedComponents = new HashSet<BiconnectedComponent>();
            }
            return biconnectedComponents;
        }

        #endregion

        public void CopyFrom(CellState source)
        {
            OccupationStatus = source.OccupationStatus;
            MoveNumber = source.MoveNumber;
            DistanceFromYou = source.DistanceFromYou;
            DistanceFromOpponent = source.DistanceFromOpponent;
            ClosestPlayer = source.ClosestPlayer;
            DegreeOfVertex = source.DegreeOfVertex;
            CompartmentStatus = source.CompartmentStatus;

            // Biconnected components properties:
            Visited = source.Visited;
            DfsDepth = source.DfsDepth;
            DfsLow = source.DfsLow;
            BiconnectedComponentsListing = source.BiconnectedComponentsListing;
        }

        public void Flip()
        {
            switch (OccupationStatus)
            {
                case OccupationStatus.You:
                    OccupationStatus = OccupationStatus.Opponent;
                    break;

                case OccupationStatus.Opponent:
                    OccupationStatus = OccupationStatus.You;
                    break;

                case OccupationStatus.YourWall:
                    OccupationStatus = OccupationStatus.OpponentWall;
                    break;

                case OccupationStatus.OpponentWall:
                    OccupationStatus = OccupationStatus.YourWall;
                    break;

                default:  // case OccupationStatus.Clear:
                    break;
            }

            switch (ClosestPlayer)
            {
                case PlayerType.You:
                    ClosestPlayer = PlayerType.Opponent;
                    break;
                case PlayerType.Opponent:
                    ClosestPlayer = PlayerType.You;
                    break;
            }

            switch (CompartmentStatus)
            {
                case CompartmentStatus.InOpponentsCompartment:
                    CompartmentStatus = CompartmentStatus.InYourCompartment;
                    break;
                case CompartmentStatus.InYourCompartment:
                    CompartmentStatus = CompartmentStatus.InOpponentsCompartment;
                    break;
            }

            int newDistanceFromOpponent = DistanceFromYou;
            DistanceFromYou = DistanceFromOpponent;
            DistanceFromOpponent = newDistanceFromOpponent;
        }

        public void ClearDijkstraStateForPlayer(PlayerType playerType)
        {
            if (playerType == PlayerType.You)
            {
                if (OccupationStatus != OccupationStatus.You)
                {
                    DistanceFromYou = int.MaxValue;
                    switch (CompartmentStatus)
                    {
                        case CompartmentStatus.InYourCompartment:
                            CompartmentStatus = CompartmentStatus.InOtherCompartment;
                            break;
                        case CompartmentStatus.InSharedCompartment:
                            CompartmentStatus = CompartmentStatus.InOpponentsCompartment;
                            break;
                    }
                }
            }

            if (playerType == PlayerType.Opponent)
            {
                if (OccupationStatus != OccupationStatus.Opponent)
                {
                    DistanceFromOpponent = int.MaxValue;
                    switch (CompartmentStatus)
                    {
                        case CompartmentStatus.InOpponentsCompartment:
                            CompartmentStatus = CompartmentStatus.InOtherCompartment;
                            break;
                        case CompartmentStatus.InSharedCompartment:
                            CompartmentStatus = CompartmentStatus.InYourCompartment;
                            break;
                    }
                }
            }

            if (OccupationStatus == Core.OccupationStatus.Clear)
            {
                ClosestPlayer = PlayerType.Unknown;
            }
        }

        public CellState[] GetAdjacentCellStates()
        {
            Position[] adjacentPositions = Position.GetAdjacentPositions();
            int count = adjacentPositions.Length;
            CellState[] adjacentCellStates = new CellState[count];
            for (int i=0; i<count; i++)
            {
                adjacentCellStates[i] = GameState[adjacentPositions[i]];
            }
            return adjacentCellStates;
        }

        public override int GetHashCode()
        {
            return GameState.GetHashCode() ^ Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is CellState)
            {
                CellState otherCellState = (CellState)obj;
                return otherCellState.GameState.Equals(GameState) && otherCellState.Position.Equals(Position);
                // TODO: Consider not comparing on GameState as well, since having the hash key as Position could be useful
            }
            return false;
        }

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        private string CalculateBiconnectedComponentsListing()
        {
            return String.Join(", ", 
                GetBiconnectedComponents()
                    .OrderBy(bcComponent => bcComponent.ComponentNumber)
                    .Select(bcComponent => bcComponent.ComponentNumber.ToString())
            );
        }

    }
}
