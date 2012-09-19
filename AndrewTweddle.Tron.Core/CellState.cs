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
        [NonSerialized]
        private Metrics subtreeMetricsForYou;
        [NonSerialized]
        private Metrics subtreeMetricsForOpponent;
        [NonSerialized]
        private bool isSubTreeVisitedForYou;
        [NonSerialized]
        private bool isSubTreeVisitedForOpponent;
        [NonSerialized]
        private BiconnectedComponent entryComponentForYou;
        [NonSerialized]
        private BiconnectedComponent exitComponentForYou;
        [NonSerialized]
        private BiconnectedComponent entryComponentForOpponent;
        [NonSerialized]
        private BiconnectedComponent exitComponentForOpponent;

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

        public Metrics SubtreeMetricsForYou
        {
            get
            {
                return subtreeMetricsForYou;
            }
            set
            {
                subtreeMetricsForYou = value;
#if DEBUG
                OnPropertyChanged("SubtreeMetricsForYou");
#endif
            }
        }

        public Metrics SubtreeMetricsForOpponent
        {
            get
            {
                return subtreeMetricsForOpponent;
            }
            set
            {
                subtreeMetricsForOpponent = value;
#if DEBUG
                OnPropertyChanged("SubtreeMetricsForOpponent");
#endif
            }
        }

        public BiconnectedComponent EntryComponentForYou
        {
            get
            {
                return entryComponentForYou;
            }
            set
            {
                entryComponentForYou = value;
#if DEBUG
                OnPropertyChanged("EntryComponentForYou");
#endif
            }
        }

        public BiconnectedComponent ExitComponentForYou
        {
            get
            {
                return exitComponentForYou;
            }
            set
            {
                exitComponentForYou = value;
#if DEBUG
                OnPropertyChanged("ExitComponentForYou");
#endif
            }
        }

        public BiconnectedComponent EntryComponentForOpponent
        {
            get
            {
                return entryComponentForOpponent;
            }
            set
            {
                entryComponentForOpponent = value;
#if DEBUG
                OnPropertyChanged("EntryComponentForOpponent");
#endif
            }
        }

        public BiconnectedComponent ExitComponentForOpponent
        {
            get
            {
                return exitComponentForOpponent;
            }
            set
            {
                exitComponentForOpponent = value;
#if DEBUG
                OnPropertyChanged("ExitComponentForOpponent");
#endif
            }
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
                        existingComponent.AddCutVertex(this);
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

        public void ClearBiconnectedComponentProperties()
        {
            Visited = false;
            ParentCellState = null;
            biconnectedComponents = null;
            IsSubTreeVisitedForYou = false;
            IsSubTreeVisitedForOpponent = false;
            BiconnectedComponentsListing = String.Empty;
            EntryComponentForYou = null;
            ExitComponentForYou = null;
            EntryComponentForOpponent = null;
            ExitComponentForOpponent = null;
            SubtreeMetricsForYou = Metrics.Zero;
            SubtreeMetricsForOpponent = Metrics.Zero;
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

        public void CalculateSubtreeMetricsForYou(MetricsEvaluator evaluator, BiconnectedComponent entryComponent)
        {
            EntryComponentForYou = entryComponent;
            BiconnectedComponent bestComponent = null;
            double valueOfBestComponent = double.NegativeInfinity;
            int branchCount = 0;

            // TODO: Order the components from most promising to least promising, in case there is a cycle:
            foreach (BiconnectedComponent component in biconnectedComponents)
            {
                if (component != entryComponent) // TODO: && component.OccupationStatus != OccupationStatus.Opponent)
                {
                    if (component.IsSubTreeVisitedForYou)
                    {
                        System.Diagnostics.Debug.WriteLine("Component {0} not being visited, as it has been visited previously for You");
                    }
                    else
                    {
                        component.CalculateSubtreeMetricsForYou(evaluator, this);
                        Metrics componentSubtreeMetrics = component.SubtreeMetricsForYou;

                        double valueOfComponent = evaluator.Evaluate(componentSubtreeMetrics);
                        if (valueOfComponent >= valueOfBestComponent)
                        {
                            valueOfBestComponent = valueOfComponent;
                            bestComponent = component;
                        }
                        branchCount += componentSubtreeMetrics.NumberOfComponentBranchesInTree;
                    }
                }
            }

            // If this is a leaf cut vertex, then its branchCount is 1:
            if (branchCount == 0)
            {
                branchCount = 1;
            }

            ExitComponentForYou = bestComponent;
            Metrics yourMetricsForCutVertexOnly = CalculateYourMetricsForCellStateOnly();
            if (bestComponent == null)
            {
                subtreeMetricsForYou = yourMetricsForCutVertexOnly;
            }
            else
            {
                subtreeMetricsForYou = yourMetricsForCutVertexOnly + bestComponent.SubtreeMetricsForYou;
            }

            subtreeMetricsForYou.NumberOfComponentBranchesInTree = branchCount;
            IsSubTreeVisitedForYou = true;
        }

        public void CalculateSubtreeMetricsForOpponent(MetricsEvaluator evaluator, BiconnectedComponent entryComponent)
        {
            EntryComponentForOpponent = entryComponent;
            BiconnectedComponent bestComponent = null;
            double valueOfBestComponent = double.NegativeInfinity;
            int branchCount = 0;

            // TODO: Order the components from most promising to least promising, in case there is a cycle:
            foreach (BiconnectedComponent component in biconnectedComponents)
            {
                if (component != entryComponent) // TODO: && component.OccupationStatus != OccupationStatus.You)
                {
                    if (component.IsSubTreeVisitedForOpponent)
                    {
                        System.Diagnostics.Debug.WriteLine("Component {0} not being visited, as it has been visited previously for Opponent");
                    }
                    else
                    {
                        component.CalculateSubtreeMetricsForOpponent(evaluator, this);
                        Metrics componentSubtreeMetrics = component.SubtreeMetricsForOpponent;

                        double valueOfComponent = evaluator.Evaluate(componentSubtreeMetrics);
                        if (valueOfComponent >= valueOfBestComponent)
                        {
                            valueOfBestComponent = valueOfComponent;
                            bestComponent = component;
                        }
                        branchCount += componentSubtreeMetrics.NumberOfComponentBranchesInTree;
                    }
                }
            }

            // If this is a leaf cut vertex, then its branchCount is 1:
            if (branchCount == 0)
            {
                branchCount = 1;
            }

            ExitComponentForOpponent = bestComponent;
            Metrics opponentsMetricsForCutVertexOnly = CalculateOpponentsMetricsForCellStateOnly();
            if (bestComponent == null)
            {
                subtreeMetricsForOpponent = opponentsMetricsForCutVertexOnly;
            }
            else
            {
                subtreeMetricsForOpponent = opponentsMetricsForCutVertexOnly + bestComponent.SubtreeMetricsForOpponent;
            }

            subtreeMetricsForOpponent.NumberOfComponentBranchesInTree = branchCount;
            IsSubTreeVisitedForOpponent = true;
        }

        private Metrics CalculateYourMetricsForCellStateOnly()
        {
            // Cap distances at 900, otherwise unreachable cells / light cycle's cell is int.MaxValue, which makes all other values meaningless:
            int cappedDistanceFromYou = DistanceFromYou > 900 ? 900 : DistanceFromYou;
            int cappedDistanceFromOpponent = DistanceFromOpponent > 900 ? 900 : DistanceFromOpponent;

            Metrics metrics = new Metrics();
            if (ClosestPlayer == PlayerType.You)
            {
                metrics.NumberOfCellsClosestToPlayer = 1;
                metrics.TotalDegreesOfCellsClosestToPlayer = DegreeOfVertex;
                metrics.SumOfDistancesFromThisPlayerOnClosestCells = cappedDistanceFromYou;
                metrics.SumOfDistancesFromOtherPlayerOnClosestCells = cappedDistanceFromOpponent;
            }
            if (CompartmentStatus == CompartmentStatus.InYourCompartment || CompartmentStatus == CompartmentStatus.InSharedCompartment)
            {
                metrics.NumberOfCellsReachableByPlayer = 1;
                metrics.TotalDegreesOfCellsReachableByPlayer = DegreeOfVertex;
            }
            return metrics;
        }

        private Metrics CalculateOpponentsMetricsForCellStateOnly()
        {
            // Cap distances at 900, otherwise unreachable cells / light cycle's cell is int.MaxValue, which makes all other values meaningless:
            int cappedDistanceFromYou = DistanceFromYou > 900 ? 900 : DistanceFromYou;
            int cappedDistanceFromOpponent = DistanceFromOpponent > 900 ? 900 : DistanceFromOpponent;

            Metrics metrics = new Metrics();
            if (ClosestPlayer == PlayerType.Opponent)
            {
                metrics.NumberOfCellsClosestToPlayer = 1;
                metrics.TotalDegreesOfCellsClosestToPlayer = DegreeOfVertex;
                metrics.SumOfDistancesFromThisPlayerOnClosestCells = cappedDistanceFromOpponent;
                metrics.SumOfDistancesFromOtherPlayerOnClosestCells = cappedDistanceFromYou;
            }
            if (CompartmentStatus == CompartmentStatus.InOpponentsCompartment || CompartmentStatus == CompartmentStatus.InSharedCompartment)
            {
                metrics.NumberOfCellsReachableByPlayer = 1;
                metrics.TotalDegreesOfCellsReachableByPlayer = DegreeOfVertex;
            }
            return metrics;
        }
    }
}
