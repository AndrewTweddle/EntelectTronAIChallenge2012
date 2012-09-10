using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

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
                OnPropertyChanged("GameState");
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
                OnPropertyChanged("Position");
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
                OnPropertyChanged("MoveNumber");
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
                    OnPropertyChanged("OccupationStatus");

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
                OnPropertyChanged("DistanceFromYou");
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
                OnPropertyChanged("DistanceFromOpponent");
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
                OnPropertyChanged("ClosestPlayer");
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
                OnPropertyChanged("DegreeOfVertex");
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
                OnPropertyChanged("CompartmentStatus");
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
                // For performance: OnPropertyChanged("Visited");
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
                // For performance: OnPropertyChanged("DfsDepth");
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
                // For performance: OnPropertyChanged("DfsLow");
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
                // For performance: OnPropertyChanged("ParentCellState");
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
            biconnectedComponents.Add(component);
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
    }
}
