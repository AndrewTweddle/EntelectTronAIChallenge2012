﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using AndrewTweddle.Tron.Core.Algorithms;
using System.ComponentModel;
using AndrewTweddle.Tron.Core.Evaluators;

namespace AndrewTweddle.Tron.Core
{
    public delegate void GameStateEvent(GameState gameState);

    [Serializable]
    public class GameState: INotifyPropertyChanged
    {
        private CellState[,] cells;
        private PlayerType playerWhoMovedFirst;
        private PlayerType playerToMoveNext;
        private CellState southPole;
        private CellState northPole;
        private CellState yourCell;
        private CellState opponentsCell;
        private CellState yourOriginalCell;
        private CellState opponentsOriginalCell;
        private int yourWallLength;
        private int opponentsWallLength;
        private bool opponentIsInSameCompartment;
        private int numberOfCellsReachableByYou;
        private int numberOfCellsReachableByOpponent;
        private int totalDegreesOfCellsReachableByYou;
        private int totalDegreesOfCellsReachableByOpponent;
        private int numberOfCellsClosestToYou;
        private int numberOfCellsClosestToOpponent;
        private int totalDegreesOfCellsClosestToYou;
        private int totalDegreesOfCellsClosestToOpponent;
        private DijkstraStatus yourDijkstraStatus;
        private DijkstraStatus opponentsDijkstraStatus;
        private int yourUpToDateDijkstraDistance;
        private int opponentsUpToDateDijkstraDistance;

        [NonSerialized]
        private List<CellState> yourFrontierCells;

        [NonSerialized]
        private List<CellState> opponentsFrontierCells;

        // Cache data that is used frequently
        [NonSerialized]
        private CellState[] allCellStates = null;

        [OptionalField]
        private int sumOfDistancesFromYouOnYourClosestCells;
        [OptionalField]
        private int sumOfDistancesFromOpponentOnYourClosestCells;
        [OptionalField]
        private int sumOfDistancesFromYouOnOpponentsClosestCells;
        [OptionalField]
        private int sumOfDistancesFromOpponentOnOpponentsClosestCells;
        [OptionalField]
        private int numberOfComponentBranchesInYourTree;
        [OptionalField]
        private int numberOfComponentBranchesInOpponentsTree;

        [OptionalField]
        private double chamberValueForYou;
        [OptionalField]
        private double chamberValueForOpponent;

        [NonSerialized]
        private List<BiconnectedComponent> biconnectedComponents;

        [NonSerialized]
        private List<Chamber> yourChambers;

        [NonSerialized]
        private List<Chamber> opponentsChambers;

        [OptionalField]
        private bool isUsingIncrementalDijkstra;

        [OptionalField]
        private string annotation;
        
        public GameState()
        {
            NorthPole = new CellState(this, Constants.NorthPoleX, Constants.NorthPoleY);
            SouthPole = new CellState(this, Constants.SouthPoleX, Constants.SouthPoleY);

            cells = new CellState[Constants.Columns, Constants.Rows - 2];

            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = Constants.ArcticCircleY; y <= Constants.AntarcticCircleY; y++)
                {
                    cells[x, y - 1] = new CellState(this, x, y);
                }
            }
            OpponentIsInSameCompartment = true;
        }

        [field:NonSerialized]
        public event GameStateEvent NewGameDetected;

        public PlayerType PlayerWhoMovedFirst
        {
            get
            {
                return playerWhoMovedFirst;
            }
            set
            {
                playerWhoMovedFirst = value;
#if DEBUG
                OnPropertyChanged("PlayerWhoMovedFirst");
#endif
            }
        }

        public PlayerType PlayerToMoveNext
        {
            get
            {
                return playerToMoveNext;
            }
            set
            {
                playerToMoveNext = value;
#if DEBUG
                OnPropertyChanged("PlayerToMoveNext");
#endif
            }
        }

        public CellState SouthPole
        {
            get
            {
                return southPole;
            }
            private set
            {
                southPole = value;
#if DEBUG
                OnPropertyChanged("SouthPole");
#endif
            }
        }

        public CellState NorthPole
        {
            get
            {
                return northPole;
            }
            private set
            {
                northPole = value;
#if DEBUG
                OnPropertyChanged("NorthPole");
#endif
            }
        }

        public CellState this[int x, int y]
        {
            get
            {
                Position pos = new Position(x, y);
                return this[pos];
            }
        }

        public CellState this[Position pos]
        {
            get
            {
                if (pos.Y == Constants.NorthPoleY)
                {
                    return northPole;
                }
                if (pos.Y == Constants.SouthPoleY)
                {
                    return southPole;
                }
                return cells[pos.X, pos.Y-1];
            }
        }

        public CellState YourCell
        {
            get
            {
                return yourCell;
            }
            set
            {
                yourCell = value;
#if DEBUG
                OnPropertyChanged("YourCell");
#endif
            }
        }

        public CellState OpponentsCell
        {
            get
            {
                return opponentsCell;
            }
            set
            {
                opponentsCell = value;
#if DEBUG
                OnPropertyChanged("OpponentsCell");
#endif
            }
        }

        public CellState YourOriginalCell
        {
            get
            {
                return yourOriginalCell;
            }
            set
            {
                yourOriginalCell = value;
#if DEBUG
                OnPropertyChanged("YourOriginalCell");
#endif
            }
        }

        public CellState OpponentsOriginalCell
        {
            get
            {
                return opponentsOriginalCell;
            }
            set
            {
                opponentsOriginalCell = value;
#if DEBUG
                OnPropertyChanged("OpponentsOriginalCell");
#endif
            }
        }

        public int YourWallLength
        {
            get
            {
                return yourWallLength;
            }
            set
            {
                yourWallLength = value;
#if DEBUG
                OnPropertyChanged("YourWallLength");
#endif
            }
        }

        public int OpponentsWallLength
        {
            get
            {
                return opponentsWallLength;
            }
            set
            {
                opponentsWallLength = value;
#if DEBUG
                OnPropertyChanged("OpponentsWallLength");
#endif
            }
        }

        public bool IsGameOver
        {
            get
            {
                return !GetPossibleNextPositions().Any();
            }
        }

        public PlayerType Winner
        {
            get
            {
                if (IsGameOver)
                {
                    return PlayerToMoveNext == PlayerType.You ? PlayerType.Opponent : PlayerType.You;
                }
                return PlayerType.Unknown;
            }
        }

        #region Dijkstra and Voronoi information

        public bool OpponentIsInSameCompartment
        {
            get
            {
                return opponentIsInSameCompartment;
            }
            set
            {
                opponentIsInSameCompartment = value;
#if DEBUG
                OnPropertyChanged("OpponentIsInSameCompartment");
#endif
            }
        }

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

        public int SumOfDistancesFromYouOnYourClosestCells
        {
            get
            {
                return sumOfDistancesFromYouOnYourClosestCells;
            }
            set
            {
                sumOfDistancesFromYouOnYourClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromYouOnYourClosestCells");
#endif
            }
        }

        public int SumOfDistancesFromOpponentOnYourClosestCells
        {
            get
            {
                return sumOfDistancesFromOpponentOnYourClosestCells;
            }
            set
            {
                sumOfDistancesFromOpponentOnYourClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromOpponentOnYourClosestCells");
#endif
            }
        }

        public int SumOfDistancesFromYouOnOpponentsClosestCells
        {
            get
            {
                return sumOfDistancesFromYouOnOpponentsClosestCells;
            }
            set
            {
                sumOfDistancesFromYouOnOpponentsClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromYouOnOpponentsClosestCells");
#endif
            }
        }

        public int SumOfDistancesFromOpponentOnOpponentsClosestCells
        {
            get
            {
                return sumOfDistancesFromOpponentOnOpponentsClosestCells;
            }
            set
            {
                sumOfDistancesFromOpponentOnOpponentsClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromOpponentOnOpponentsClosestCells");
#endif
            }
        }

        public int TotalDegreesOfCellsClosestToYou 
        { 
            get
            {
                return totalDegreesOfCellsClosestToYou ; 
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

        public int NumberOfComponentBranchesInYourTree
        {
            get
            {
                return numberOfComponentBranchesInYourTree;
            }
            set
            {
                numberOfComponentBranchesInYourTree = value;
#if DEBUG
                OnPropertyChanged("NumberOfComponentBranchesInYourTree");
#endif
            }
        }

        public int NumberOfComponentBranchesInOpponentsTree
        {
            get
            {
                return numberOfComponentBranchesInOpponentsTree;
            }
            set
            {
                numberOfComponentBranchesInOpponentsTree = value;
#if DEBUG
                OnPropertyChanged("NumberOfComponentBranchesInOpponentsTree");
#endif
            }
        }

        public double ChamberValueForYou
        {
            get
            {
                return chamberValueForYou;
            }
            set
            {
                chamberValueForYou = value;
#if DEBUG
                OnPropertyChanged("ChamberValueForYou");
#endif
            }
        }

        public double ChamberValueForOpponent
        {
            get
            {
                return chamberValueForOpponent;
            }
            set
            {
                chamberValueForOpponent = value;
#if DEBUG
                OnPropertyChanged("ChamberValueForOpponent");
#endif
            }
        }

        public DijkstraStatus YourDijkstraStatus
        {
            get
            {
                return yourDijkstraStatus;
            }
            set
            {
                yourDijkstraStatus = value;
#if DEBUG
                OnPropertyChanged("YourDijkstraStatus");
#endif
            }
        }

        public DijkstraStatus OpponentsDijkstraStatus
        {
            get
            {
                return opponentsDijkstraStatus;
            }
            set
            {
                opponentsDijkstraStatus = value;
#if DEBUG
                OnPropertyChanged("OpponentsDijkstraStatus");
#endif
            }
        }

        public int YourUpToDateDijkstraDistance
        {
            get
            {
                return yourUpToDateDijkstraDistance;
            }
            set
            {
                yourUpToDateDijkstraDistance = value;
#if DEBUG
                OnPropertyChanged("YourUpToDateDijkstraDistance");
#endif
            }
        }

        public int OpponentsUpToDateDijkstraDistance
        {
            get
            {
                return opponentsUpToDateDijkstraDistance;
            }
            set
            {
                opponentsUpToDateDijkstraDistance = value;
#if DEBUG
                OnPropertyChanged("OpponentsUpToDateDijkstraDistance");
#endif
            }
        }

        public bool IsUsingIncrementalDijkstra
        {
            get
            {
                return isUsingIncrementalDijkstra;
            }
            set
            {
                isUsingIncrementalDijkstra = value;
#if DEBUG
                OnPropertyChanged("IsUsingIncrementalDijkstra");
#endif
            }
        }

        #endregion

        #region Diagnostic information

        public string Annotation
        {
            get
            {
                return annotation;
            }
            set
            {
                annotation = value;
#if DEBUG
                OnPropertyChanged("Annotation");
#endif
            }
        }

        #endregion

        public static GameState LoadGameState(string filePath, FileType fileType = FileType.Binary)
        {
            IFormatter formatter;
            switch (fileType)
            {
                case FileType.Xml:
                    formatter = new SoapFormatter();
                    break;

                default:
                    formatter = new BinaryFormatter();
                    break;
            }
            using (FileStream fs = File.OpenRead(filePath))
            {
                GameState gameState = (GameState) formatter.Deserialize(fs);
                return gameState;
            }
        }

        public void SaveGameState(string filePath, FileType fileType = FileType.Binary)
        {
            IFormatter formatter;
            switch (fileType)
            {
                case FileType.Xml:
                    formatter = new SoapFormatter();
                    break;

                default:
                    formatter = new BinaryFormatter();
                    break;
            }

            Stopwatch swatch = Stopwatch.StartNew();

            using (FileStream fs = File.Create(filePath))
            {
                formatter.Serialize(fs, this);
            }

            swatch.Stop();
            Debug.WriteLine("Time to serialize state: {0}", swatch.Elapsed);
        }

        public void LoadRawCellData(IEnumerable<RawCellData> cells, PlayerType newPlayerToMoveNext = PlayerType.You)
        {
            bool isFirstNorthPoleCell = true;
            bool isFirstSouthPoleCell = true;
            int yourWallLength = 0;
            int opponentsWallLength = 0;
            foreach (RawCellData cellData in cells)
            {
                CellState cellState = this[cellData.X, cellData.Y];
                cellState.OccupationStatus = cellData.OccupationStatus;

                bool increaseWallLength = true;

                if (cellState.Position.IsNorthPole)
                {
                    if (isFirstNorthPoleCell)
                    {
                        isFirstNorthPoleCell = false;
                    }
                    else
                    {
                        increaseWallLength = false;
                    }
                }

                if (cellState.Position.IsSouthPole)
                {
                    if (isFirstSouthPoleCell)
                    {
                        isFirstSouthPoleCell = false;
                    }
                    else
                    {
                        increaseWallLength = false;
                    }
                }

                if (increaseWallLength)
                {
                    switch (cellData.OccupationStatus)
                    {
                        case OccupationStatus.YourWall:
                            yourWallLength++;
                            break;
                        case OccupationStatus.OpponentWall:
                            opponentsWallLength++;
                            break;
                        case OccupationStatus.You:
                            YourCell = cellState;
                            break;
                        case OccupationStatus.Opponent:
                            OpponentsCell = cellState;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (YourWallLength > yourWallLength && OpponentsWallLength > opponentsWallLength)
            {
                // This must be a brand new game. So clear all data carried forward from previous game.
                ClearInheritedData();
                if (NewGameDetected != null)
                {
                    NewGameDetected(this);
                }
            }

            YourWallLength = yourWallLength;
            OpponentsWallLength = opponentsWallLength;
            PlayerToMoveNext = newPlayerToMoveNext;

            if (yourWallLength == opponentsWallLength)
            {
                PlayerWhoMovedFirst = PlayerType.You;
                if (yourWallLength == 0)
                {
                    YourOriginalCell = YourCell;
                    OpponentsOriginalCell = OpponentsCell;
                }
            }
            else
                if (opponentsWallLength == yourWallLength + 1 && newPlayerToMoveNext == PlayerType.Opponent)
                {
                    PlayerWhoMovedFirst = PlayerType.Opponent;
                    if (yourWallLength == 0)
                    {
                        YourOriginalCell = YourCell;
                        OpponentsOriginalCell = OpponentsCell.GetAdjacentCellStates().FirstOrDefault(
                            cs => cs.OccupationStatus == OccupationStatus.OpponentWall);
                    }
                }
                else 
                    if (yourWallLength == opponentsWallLength + 1 && newPlayerToMoveNext == PlayerType.Opponent)
                    {
                        PlayerWhoMovedFirst = PlayerType.You;
                        if (opponentsWallLength == 0)
                        {
                            OpponentsOriginalCell = OpponentsCell;
                            YourOriginalCell = YourCell.GetAdjacentCellStates().FirstOrDefault(
                                cs => cs.OccupationStatus == OccupationStatus.YourWall);
                        }
                    }
                    else
                    {
                        string errorMessage = string.Format(
                            "Wall lengths are not consistent with alternating player turns (you: {0}, opponent: {1})",
                            yourWallLength, opponentsWallLength);
                        throw new ApplicationException(errorMessage);
                    }

            // Set move numbers on each cell (starting position is zero, then increments for each player):
            YourCell.MoveNumber = yourWallLength;
            OpponentsCell.MoveNumber = opponentsWallLength;
        }

        private void ClearInheritedData(bool resetMoveNumbers = true)
        {
            if (resetMoveNumbers)
            {
                foreach (CellState cellState in GetAllCellStates())
                {
                    cellState.MoveNumber = 0;
                }
            }
            ClearDijkstraProperties();
        }

        public void ClearDijkstraProperties()
        {
            OpponentIsInSameCompartment = true;

            if (yourFrontierCells != null)
            {
                yourFrontierCells.Clear();
            }

            if (opponentsFrontierCells != null)
            {
                opponentsFrontierCells.Clear();
            }

            NumberOfCellsReachableByYou = 0;
            TotalDegreesOfCellsReachableByYou = 0;
            NumberOfCellsClosestToYou = 0;
            TotalDegreesOfCellsClosestToYou = 0;
            YourDijkstraStatus = DijkstraStatus.NotCalculated;
            YourUpToDateDijkstraDistance = 0;

            NumberOfCellsReachableByOpponent = 0;
            TotalDegreesOfCellsReachableByOpponent = 0;
            NumberOfCellsClosestToOpponent = 0;
            TotalDegreesOfCellsClosestToOpponent = 0;
            OpponentsDijkstraStatus = DijkstraStatus.NotCalculated;
            OpponentsUpToDateDijkstraDistance = 0;

            CellState[] cells = GetAllCellStates();
            foreach (CellState cell in cells)
            {
                switch (cell.OccupationStatus)
                {
                    case OccupationStatus.Clear:
                        cell.DistanceFromYou = int.MaxValue;
                        cell.DistanceFromOpponent = int.MaxValue;
                        cell.ClosestPlayer = PlayerType.Unknown;
                        cell.CompartmentStatus = CompartmentStatus.InOtherCompartment;
                        break;
                    case OccupationStatus.You:
                        cell.DistanceFromYou = 0;
                        cell.DistanceFromOpponent = int.MaxValue;
                        cell.ClosestPlayer = PlayerType.You;
                        cell.CompartmentStatus = CompartmentStatus.InYourCompartment;
                        break;
                    case OccupationStatus.Opponent:
                        cell.DistanceFromOpponent = 0;
                        cell.DistanceFromYou = int.MaxValue;
                        cell.ClosestPlayer = PlayerType.Opponent;
                        cell.CompartmentStatus = CompartmentStatus.InOpponentsCompartment;
                        break;
                }
            }
        }

        public void ClearDijkstraPropertiesForPlayer(PlayerType playerType)
        {
            // TODO: Why still here...
            // OpponentIsInSameCompartment = true;

            if (playerType == PlayerType.You)
            {
                NumberOfCellsReachableByYou = 0;
                TotalDegreesOfCellsReachableByYou = 0;
                NumberOfCellsClosestToYou = 0;
                TotalDegreesOfCellsClosestToYou = 0;
                YourDijkstraStatus = DijkstraStatus.NotCalculated;
                YourUpToDateDijkstraDistance = 0;
            }

            if (playerType == PlayerType.Opponent)
            {
                NumberOfCellsReachableByOpponent = 0;
                TotalDegreesOfCellsReachableByOpponent = 0;
                NumberOfCellsClosestToOpponent = 0;
                TotalDegreesOfCellsClosestToOpponent = 0;
                OpponentsDijkstraStatus = DijkstraStatus.NotCalculated;
                OpponentsUpToDateDijkstraDistance = 0;
            }

            CellState[] cells = GetAllCellStates();
            foreach (CellState cell in cells)
            {
                cell.ClearDijkstraStateForPlayer(playerType);
            }
        }

        public void AddFrontierCellForPlayer(CellState cellState, PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.You:
                    if (yourFrontierCells == null)
                    {
                        yourFrontierCells = new List<CellState>();
                    }
                    yourFrontierCells.Add(cellState);
                    break;

                case PlayerType.Opponent:
                    if (opponentsFrontierCells == null)
                    {
                        opponentsFrontierCells = new List<CellState>();
                    }
                    opponentsFrontierCells.Add(cellState);
                    break;
            }
        }

        public IEnumerable<CellState> GetFrontierCellsForPlayer(PlayerType player)
        {
            switch (player)
            {
                case PlayerType.You:
                    if (yourFrontierCells == null)
                    {
                        yourFrontierCells = new List<CellState>();
                    }
                    return yourFrontierCells;

                default:  // Assume Opponent
                    if (opponentsFrontierCells == null)
                    {
                        opponentsFrontierCells = new List<CellState>();
                    }
                    return opponentsFrontierCells;
            }
        }

        public void LoadTronGameFile(string filePath)
        {
            IEnumerable<RawCellData> cells = LoadRawCellDataFromTronGameFile(filePath);
            LoadRawCellData(cells);
        }

        public static IEnumerable<RawCellData> LoadRawCellDataFromTronGameFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Regex rgx = new Regex(RegexConstants.TronGameFileRegularExpression, RegexOptions.None);
            int lineNumber = 0;

            foreach (string line in lines)
            {
                lineNumber++;
                Match match = rgx.Match(line);
                if (!match.Success)
                {
                    // Ignore lines which are all whitespace:
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // Otherwise throw an error:
                    string errorMessage = String.Format("Error in line {0:3}: {1}", lineNumber, line);
                    throw new ApplicationException(errorMessage);
                }
                int x = int.Parse(match.Groups[RegexConstants.XCaptureGroupName].Value);
                int y = int.Parse(match.Groups[RegexConstants.YCaptureGroupName].Value);
                string occupationStatusName = match.Groups[RegexConstants.OccupationStatusCaptureGroupName].Value;
                OccupationStatus occupationStatus = (OccupationStatus) Enum.Parse(typeof(OccupationStatus), occupationStatusName);

                yield return new RawCellData
                {
                    X = x,
                    Y = y,
                    OccupationStatus = occupationStatus
                };
            }
        }

        public void SaveTronGameFile(string filePath)
        {
            List<string> lines = new List<string>();

            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = 0; y < Constants.Rows; y++)
                {
                    CellState cellState = this[x, y];
                    string line = String.Format("{0} {1} {2}", x, y, cellState.OccupationStatus.ToString());
                    lines.Add(line);
                }
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        public GameState Clone()
        {
            GameState clone = new GameState();
            clone.CopyFrom(this);

            return clone;
        }

        public void CopyFrom(GameState sourceGameState)
        {
            foreach (CellState source in sourceGameState.GetAllCellStates())
            {
                CellState dest = this[source.Position];
                dest.CopyFrom(source);
            }

            if (sourceGameState.OpponentsCell == null)
            {
                OpponentsCell = null;
            }
            else
            {
                OpponentsCell = this[sourceGameState.OpponentsCell.Position];
            }

            if (sourceGameState.YourCell == null)
            {
                YourCell = null;
            }
            else
            {
                YourCell = this[sourceGameState.YourCell.Position];
            }

            if (sourceGameState.OpponentsOriginalCell == null)
            {
                OpponentsOriginalCell = null;
            }
            else
            {
                OpponentsOriginalCell = this[sourceGameState.OpponentsOriginalCell.Position];
            }

            if (sourceGameState.YourOriginalCell == null)
            {
                YourOriginalCell = null;
            }
            else
            {
                YourOriginalCell = this[sourceGameState.YourOriginalCell.Position];
            }

            YourWallLength = sourceGameState.YourWallLength;
            OpponentsWallLength = sourceGameState.OpponentsWallLength;
            PlayerWhoMovedFirst = sourceGameState.PlayerWhoMovedFirst;
            PlayerToMoveNext = sourceGameState.PlayerToMoveNext;

            OpponentIsInSameCompartment = sourceGameState.OpponentIsInSameCompartment;
            NumberOfCellsReachableByYou = sourceGameState.NumberOfCellsReachableByYou;
            NumberOfCellsReachableByOpponent = sourceGameState.NumberOfCellsReachableByOpponent;
            TotalDegreesOfCellsReachableByYou = sourceGameState.TotalDegreesOfCellsReachableByYou;
            TotalDegreesOfCellsReachableByOpponent = sourceGameState.TotalDegreesOfCellsReachableByOpponent;
            NumberOfCellsClosestToYou = sourceGameState.NumberOfCellsClosestToYou;
            NumberOfCellsClosestToOpponent = sourceGameState.NumberOfCellsClosestToOpponent;
            TotalDegreesOfCellsClosestToYou = sourceGameState.TotalDegreesOfCellsClosestToYou;
            TotalDegreesOfCellsClosestToOpponent = sourceGameState.TotalDegreesOfCellsClosestToOpponent;
            SumOfDistancesFromYouOnYourClosestCells = sourceGameState.sumOfDistancesFromYouOnYourClosestCells;
            SumOfDistancesFromYouOnOpponentsClosestCells = sourceGameState.SumOfDistancesFromYouOnOpponentsClosestCells;
            SumOfDistancesFromOpponentOnOpponentsClosestCells = sourceGameState.SumOfDistancesFromOpponentOnOpponentsClosestCells;
            SumOfDistancesFromOpponentOnYourClosestCells = sourceGameState.SumOfDistancesFromOpponentOnYourClosestCells;
            ChamberValueForYou = sourceGameState.ChamberValueForYou;
            ChamberValueForOpponent = sourceGameState.ChamberValueForOpponent;
            YourDijkstraStatus = sourceGameState.YourDijkstraStatus;
            OpponentsDijkstraStatus = sourceGameState.OpponentsDijkstraStatus;
            YourUpToDateDijkstraDistance = sourceGameState.YourUpToDateDijkstraDistance;
            OpponentsUpToDateDijkstraDistance = sourceGameState.OpponentsUpToDateDijkstraDistance;
            Annotation = sourceGameState.Annotation;

            IsUsingIncrementalDijkstra = sourceGameState.IsUsingIncrementalDijkstra;
        }

        public CellState[] GetAllCellStates()
        {
            if (allCellStates == null)
            {
                int numberOfCellStates = (Constants.Rows - 2) * Constants.Columns + 2;
                allCellStates = new CellState[numberOfCellStates];
                allCellStates[0] = NorthPole;
                int i = 1;
                foreach (CellState cellState in cells)
                {
                    allCellStates[i] = cellState;
                    i++;
                }
                allCellStates[i] = SouthPole;
            }
            return allCellStates;
        }

        public CellState GetNextCellStateForPlayerFromPreviousCellState(CellState previousCellStateForPlayer)
        {
            int moveNumber = previousCellStateForPlayer.MoveNumber + 1;

            OccupationStatus playerWall;
            OccupationStatus playerCycle;

            switch (previousCellStateForPlayer.OccupationStatus)
            {
                case OccupationStatus.You:
                case OccupationStatus.YourWall:
                    playerWall = OccupationStatus.YourWall;
                    playerCycle = OccupationStatus.You;
                    break;
                case OccupationStatus.Opponent:
                case OccupationStatus.OpponentWall:
                    playerWall = OccupationStatus.OpponentWall;
                    playerCycle = OccupationStatus.Opponent;
                    break;
                default:
                    return null;
            }
            foreach (CellState cellState in previousCellStateForPlayer.GetAdjacentCellStates())
            {
                if (cellState.MoveNumber == moveNumber && (cellState.OccupationStatus == playerWall || cellState.OccupationStatus == playerCycle))
                {
                    return cellState;
                }
            }
            return null;
        }

        public CellState GetCellByMoveNumber(PlayerType player, int moveNumber)
        {
            OccupationStatus playerWall;
            OccupationStatus playerCycle;
            switch (player)
            {
                case PlayerType.You:
                    playerWall = OccupationStatus.YourWall;
                    playerCycle = OccupationStatus.You;
                    break;
                case PlayerType.Opponent:
                    playerWall = OccupationStatus.OpponentWall;
                    playerCycle = OccupationStatus.Opponent;
                    break;
                default:
                    return null;
            }

            // Search all positions for the given move:
            foreach (CellState cellState in GetAllCellStates())
            {
                if (cellState.MoveNumber == moveNumber && (cellState.OccupationStatus == playerWall || cellState.OccupationStatus == playerCycle))
                {
                    return cellState;
                }
            }
            return null;
        }

        public IEnumerable<Position> GetPossibleNextPositions()
        {
            CellState fromCell;

            if (PlayerToMoveNext == PlayerType.You)
            {
                fromCell = YourCell;
            }
            else
            {
                fromCell = OpponentsCell;
            }

            IEnumerable<CellState> clearCells = fromCell.GetAdjacentCellStates();
            foreach (CellState toCell in clearCells)
            {
                if (toCell.OccupationStatus == OccupationStatus.Clear)
                {
                    yield return toCell.Position;
                }
            }
        }

        public IEnumerable<Move> GetPossibleNextMoves()
        {
            CellState fromCell;
            int moveNumber;

            if (PlayerToMoveNext == PlayerType.You)
            {
                fromCell = YourCell;
                moveNumber = YourWallLength + 1;
            }
            else
            {
                fromCell = OpponentsCell;
                moveNumber = OpponentsWallLength + 1;
            }

            IEnumerable<CellState> clearCells = fromCell.GetAdjacentCellStates();
            foreach (CellState toCell in clearCells)
            {
                if (toCell.OccupationStatus == OccupationStatus.Clear)
                {
                    Move move = new Move(PlayerToMoveNext, moveNumber, toCell.Position);
                    yield return move;
                }
            }
        }

        public IEnumerable<GameState> GetPossibleNextStates()
        {
            IEnumerable<Move> possibleNextMoves = GetPossibleNextMoves();
            foreach (Move move in possibleNextMoves)
            {
                GameState nextGameState = Clone();
                nextGameState.MakeMove(move);
                yield return nextGameState;
            }
        }

        public bool IsValidPositionToMoveTo(Position positionToMoveTo)
        {
            return GetPossibleNextPositions().Contains(positionToMoveTo);
        }

        public bool IsValidMove(Move move)
        {
            return GetPossibleNextMoves().Contains(move);
        }

        public void MakeMove(Move move, bool performDijkstra = true, bool shouldCalculatedBiconnectedComponents = true)
        {
            // Check that the move has the correct player:
            if (move.PlayerType != PlayerToMoveNext)
            {
                string errorMessage = String.Format("Attempt to {0} is invalid because it's the other player's turn", move);
                throw new InvalidOperationException(errorMessage);
            }

            // Check that the move has the correct move number:
            int expectedMoveNumber = 1 + (move.PlayerType == PlayerType.You ? YourWallLength : OpponentsWallLength);
            if (move.MoveNumber != expectedMoveNumber)
            {
                string errorMessage = String.Format(
                    "Attempt to {0} is invalid because the turn number should be {1}",
                    move, expectedMoveNumber);
                throw new InvalidOperationException(errorMessage);
            }

            // Further validation will occur in the MoveToPosition call:
            MoveToPosition(move.To, performDijkstra, shouldCalculatedBiconnectedComponents);
        }

        public Move GetLastMove()
        {
            Move move;

            if (PlayerToMoveNext == PlayerType.You)
            {
                // Is it the start of the game?
                if (OpponentsWallLength == 0)
                {
                    return null;
                }

                // Opponent moved last:
                move = new Move(playerType: PlayerType.Opponent, moveNumber: OpponentsWallLength, to: OpponentsCell.Position);
                return move;
            }

            // Is it the start of the game?
            if (YourWallLength == 0)
            {
                return null;
            }

            // You moved last:
            move = new Move(playerType: PlayerType.You, moveNumber: YourWallLength, to: YourCell.Position);
            return move;
        }

        public void MoveToPosition(Position position, bool performDijkstra = true, bool shouldCalculatedBiconnectedComponents = true)
        {
            CellState fromCell = PlayerToMoveNext == PlayerType.You ? YourCell : OpponentsCell;
            CellState toCell = this[position];

            if (toCell.OccupationStatus != OccupationStatus.Clear)
            {
                string errorMessage = String.Format(
                    "{0} cannot move to ({1}, {2}) as that space is occupied by {3}",
                    PlayerToMoveNext, toCell.Position.X, toCell.Position.Y, toCell.OccupationStatus);
                throw new ApplicationException(errorMessage);
            }

            if (!fromCell.Position.GetAdjacentPositions().Contains(toCell.Position))
            {
                string errorMessage = String.Format(
                    "{0} cannot move from ({1}, {2}) to ({3}, {4}) as these positions are not adjacent.",
                    PlayerToMoveNext, fromCell.Position.X, fromCell.Position.Y, toCell.Position.X, toCell.Position.Y);
                throw new ApplicationException(errorMessage);
            }

            toCell.MoveNumber = fromCell.MoveNumber + 1;

            if (PlayerToMoveNext == PlayerType.You)
            {
                fromCell.OccupationStatus = OccupationStatus.YourWall;
                toCell.OccupationStatus = OccupationStatus.You;
                PlayerToMoveNext = PlayerType.Opponent;
                YourWallLength += 1;
                YourCell = toCell;

                // Update Dijkstra status and distance:
                YourDijkstraStatus = DijkstraStatus.NotCalculated;
                YourUpToDateDijkstraDistance = 0;

                if (IsUsingIncrementalDijkstra)
                {
                    if (toCell.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        if (OpponentsDijkstraStatus == DijkstraStatus.FullyCalculated
                            || (OpponentsDijkstraStatus == DijkstraStatus.PartiallyCalculated && OpponentsUpToDateDijkstraDistance >= toCell.DistanceFromOpponent))
                        {
                            OpponentsDijkstraStatus = DijkstraStatus.PartiallyCalculated;
                            OpponentsUpToDateDijkstraDistance = toCell.DistanceFromOpponent - 1;
                        }
                    }
                    else
                    {
                        OpponentIsInSameCompartment = false;
                    }
                }
                else
                {
                    OpponentsDijkstraStatus = DijkstraStatus.NotCalculated;
                    OpponentsUpToDateDijkstraDistance = 0;
                }
            }
            else
            {
                fromCell.OccupationStatus = OccupationStatus.OpponentWall;
                toCell.OccupationStatus = OccupationStatus.Opponent;
                PlayerToMoveNext = PlayerType.You;
                OpponentsWallLength++;
                OpponentsCell = toCell;

                // Update Dijkstra status and distance:
                OpponentsDijkstraStatus = DijkstraStatus.NotCalculated;
                OpponentsUpToDateDijkstraDistance = 0;

                if (IsUsingIncrementalDijkstra)
                {
                    if (toCell.CompartmentStatus == CompartmentStatus.InSharedCompartment)
                    {
                        if (YourDijkstraStatus == DijkstraStatus.FullyCalculated
                            || (YourDijkstraStatus == DijkstraStatus.PartiallyCalculated && YourUpToDateDijkstraDistance >= toCell.DistanceFromYou))
                        {
                            YourDijkstraStatus = DijkstraStatus.PartiallyCalculated;
                            YourUpToDateDijkstraDistance = toCell.DistanceFromYou - 1;
                        }
                    }
                    else
                    {
                        OpponentIsInSameCompartment = false;
                    }
                }
                else
                {
                    YourDijkstraStatus = DijkstraStatus.NotCalculated;
                    YourUpToDateDijkstraDistance = 0;
                }
            }

            if (performDijkstra)
            {
                Dijkstra.Perform(this);
            }

            if (shouldCalculatedBiconnectedComponents)
            {
                BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                bcAlg.Calculate(this, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);
            }
        }

        public void UndoLastMove(bool performDijkstra = false, bool shouldCalculatedBiconnectedComponents = false)
        {
            CellState cellToClear;
            CellState previousCell = null;
            OccupationStatus oldStatusOfPreviousCell;
            OccupationStatus newStatusOfPreviousCell;
            PlayerType newPlayerToMoveNext;
            int lastMoveNumber;

            switch (PlayerToMoveNext)
            {
                case PlayerType.You:
                    // Opponent moved last:
                    cellToClear = OpponentsCell;
                    oldStatusOfPreviousCell = OccupationStatus.OpponentWall;
                    newStatusOfPreviousCell = OccupationStatus.Opponent;
                    newPlayerToMoveNext = PlayerType.Opponent;
                    break;

                default:
                    // You moved last:
                    cellToClear = YourCell;
                    oldStatusOfPreviousCell = OccupationStatus.YourWall;
                    newStatusOfPreviousCell = OccupationStatus.You;
                    newPlayerToMoveNext = PlayerType.You;
                    break;
            }

            lastMoveNumber = cellToClear.MoveNumber - 1;
            foreach (CellState cs in cellToClear.GetAdjacentCellStates())
            {
                if (cs.OccupationStatus == oldStatusOfPreviousCell && cs.MoveNumber == lastMoveNumber)
                {
                    previousCell = cs;
                    break;
                }
            }

            if (lastMoveNumber == -1 || previousCell == null)
            {
                throw new ApplicationException("The previous move can't be undone as there was no previous move");
            }

            previousCell.OccupationStatus = newStatusOfPreviousCell;
            cellToClear.OccupationStatus = OccupationStatus.Clear;
            cellToClear.MoveNumber = 0;
            PlayerToMoveNext = newPlayerToMoveNext;

            switch (newPlayerToMoveNext)
            {
                case PlayerType.You:
                    YourWallLength--;
                    YourCell = previousCell;
                    break;

                default:
                    OpponentsWallLength--;
                    OpponentsCell = previousCell;
                    break;
            }

            // Update Dijkstra status and distance:
            YourDijkstraStatus = DijkstraStatus.NotCalculated;
            YourUpToDateDijkstraDistance = 0;
            opponentsDijkstraStatus = DijkstraStatus.NotCalculated;
            OpponentsUpToDateDijkstraDistance = 0;

            // Perform algorithms, if requested:
            if (performDijkstra)
            {
                Dijkstra.Perform(this);
            }

            if (shouldCalculatedBiconnectedComponents)
            {
                BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                bcAlg.Calculate(this, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);
            }
        }

        public void CheckThatGameStateIsValid(GameState previousGameState)
        {
            if (PlayerToMoveNext == previousGameState.PlayerToMoveNext)
            {
                throw new ApplicationException("The player to move next did not change");
            }

            switch (previousGameState.PlayerToMoveNext)
            {
                case PlayerType.You:
                    if (YourWallLength != previousGameState.YourWallLength + 1)
                    {
                        throw new ApplicationException("Your wall length did not increase after your move");
                    }
                    break;

                case PlayerType.Opponent:
                    if (OpponentsWallLength != previousGameState.OpponentsWallLength + 1)
                    {
                        throw new ApplicationException("The opponents wall length did not increase after their move");
                    }
                    break;

                default:
                    throw new ApplicationException("Player to move next is unknown");
            }

            CellState cellThatWasVacated = null;
            CellState cellThatWasOccupied = null;

            foreach (CellState srcCell in previousGameState.GetAllCellStates())
            {
                string errorMessage;
                CellState destCell = this[srcCell.Position];

                if (srcCell.OccupationStatus != destCell.OccupationStatus)
                {
                    switch (srcCell.OccupationStatus)
                    {
                        case OccupationStatus.Clear:
                            if (cellThatWasOccupied == null)
                            {
                                cellThatWasOccupied = destCell;
                            }
                            else
                            {
                                errorMessage = String.Format(
                                    "Multiple cells were vacated in a single move: ({0}, {1}) and ({2}, {3})",
                                    cellThatWasOccupied.Position.X, cellThatWasOccupied.Position.Y,
                                    destCell.Position.X, destCell.Position.Y);
                                throw new ApplicationException(errorMessage);
                            }
                            OccupationStatus expectedOccupationStatus = previousGameState.PlayerToMoveNext.ToOccupationStatus();
                            if (destCell.OccupationStatus != expectedOccupationStatus)
                            {
                                errorMessage = String.Format("The cell occupied ({0}, {1}) was changed to {2} not {3}",
                                        destCell.Position.X, destCell.Position.Y,
                                        destCell.OccupationStatus, expectedOccupationStatus);
                                throw new ApplicationException(errorMessage);
                            }
                            break;

                        case OccupationStatus.You:
                        case OccupationStatus.Opponent:
                            /* Changed from a tron cycle to a wall. Check that there was only 1 such change: */
                            if (cellThatWasVacated == null)
                            {
                                cellThatWasVacated = destCell;
                            }
                            else
                            {
                                errorMessage = String.Format(
                                    "Multiple cells were vacated in a single move: ({0}, {1}) and ({2}, {3})",
                                    cellThatWasVacated.Position.X, cellThatWasVacated.Position.Y,
                                    destCell.Position.X, destCell.Position.Y);
                                throw new ApplicationException(errorMessage);
                            }

                            /* Check that the cell vacated was for the player who had the move: */
                            if (srcCell.OccupationStatus != previousGameState.PlayerToMoveNext.ToOccupationStatus())
                            {
                                errorMessage = String.Format(
                                    "The cell vacated at ({0}, {1}) was not for {2}, but should have been.",
                                    destCell.Position.X, destCell.Position.Y, previousGameState.PlayerToMoveNext);
                                throw new ApplicationException(errorMessage);
                            }

                            /* Check that the correct wall was put in the tron cycle's place: */
                            OccupationStatus expectedWallOccupationStatus = previousGameState.PlayerToMoveNext.ToWallType();
                            if (destCell.OccupationStatus != expectedWallOccupationStatus)
                            {
                                errorMessage = String.Format("The cell at ({0}, {1}) was changed to {2} not {3}, which is not allowed",
                                    destCell.Position.X, destCell.Position.Y,
                                    destCell.OccupationStatus, expectedWallOccupationStatus);
                                throw new ApplicationException(errorMessage);
                            }
                            break;

                        default:
                            errorMessage = String.Format(
                                "The cell at ({0}, {1}) changed from {2} to {3}, which is not allowed",
                                srcCell.Position.X, srcCell.Position.Y, srcCell.OccupationStatus, destCell.OccupationStatus);
                            throw new ApplicationException(errorMessage);
                    }
                }
            }

            /* TODO: The following could be a result of losing the game. 
             * So find a better way e.g. a VictoriousPlayer property: 
             */
            if (cellThatWasVacated == null)
            {
                throw new ApplicationException("No cell was exited");
            }

            if (cellThatWasOccupied == null)
            {
                throw new ApplicationException("No cell was occupied");
            }
        }

        public static GameState InitializeNewGameState(bool performDijkstra = true, bool shouldCalculateBiconnectedComponents = true)
        {
            Random rnd = new Random();
            int yourX = rnd.Next(Constants.Columns);
            int yourY = rnd.Next(Constants.Rows - 2) + 1;
            int opponentsX = (yourX + Constants.Columns / 2) % Constants.Columns;
            int opponentsY = yourY; // was: Constants.SouthPoleY - yourY;

            GameState gameState = InitializeNewGameState(yourX, yourY, opponentsX, opponentsY, performDijkstra, shouldCalculateBiconnectedComponents);
            return gameState;
        }

        public static GameState InitializeNewGameState(Position yourStartingPosition, Position opponentsStartingPosition,
            bool performDijkstra = true, bool shouldCalculateBiconnectedComponents = true)
        {
            return InitializeNewGameState(yourStartingPosition.X, yourStartingPosition.Y, opponentsStartingPosition.X, opponentsStartingPosition.Y,
                performDijkstra, shouldCalculateBiconnectedComponents);
        }

        public static GameState InitializeNewGameState(int yourX, int yourY, int opponentsX, int opponentsY,
            bool performDijkstra = true, bool shouldCalculateBiconnectedComponents = true)
        {
            GameState gameState = new GameState();
            CellState yourCell = gameState[yourX, yourY];
            CellState opponentsCell = gameState[opponentsX, opponentsY];
            yourCell.OccupationStatus = OccupationStatus.You;
            opponentsCell.OccupationStatus = OccupationStatus.Opponent;
            gameState.YourCell = yourCell;
            gameState.YourOriginalCell = yourCell;
            gameState.OpponentsCell = opponentsCell;
            gameState.PlayerWhoMovedFirst = PlayerType.You;
            gameState.OpponentsOriginalCell = opponentsCell;
            gameState.PlayerToMoveNext = PlayerType.You;
            if (performDijkstra)
            {
                Dijkstra.Perform(gameState);
            }
            if (shouldCalculateBiconnectedComponents)
            {
                BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                bcAlg.Calculate(gameState, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);
            }
            return gameState;
        }

        public void FlipGameState()
        {
            CellState newOpponentsCell = YourCell, newYourCell = OpponentsCell;

            foreach (CellState cellState in GetAllCellStates())
            {
                cellState.Flip();
            }

            CellState newOpponentsOriginalCell = YourOriginalCell;
            YourOriginalCell = OpponentsOriginalCell;
            OpponentsOriginalCell = newOpponentsOriginalCell;

            OpponentsCell = newOpponentsCell;
            YourCell = newYourCell;

            PlayerWhoMovedFirst = (PlayerWhoMovedFirst == PlayerType.You) ? PlayerType.Opponent : PlayerType.You;
            PlayerToMoveNext = (PlayerToMoveNext == PlayerType.You) ? PlayerType.Opponent : PlayerType.You;

            int newOpponentsWallLength = YourWallLength;
            YourWallLength = OpponentsWallLength;
            OpponentsWallLength = newOpponentsWallLength;

            int newNumberOfCellsClosestToOpponent = NumberOfCellsClosestToYou;
            NumberOfCellsClosestToYou = NumberOfCellsClosestToOpponent;
            NumberOfCellsClosestToOpponent = newNumberOfCellsClosestToOpponent;

            int newNumberOfCellsReachableByOpponent = NumberOfCellsReachableByYou;
            NumberOfCellsReachableByYou = NumberOfCellsReachableByOpponent;
            NumberOfCellsReachableByOpponent = newNumberOfCellsReachableByOpponent;

            int newTotalDegreesOfCellsClosestToOpponent = TotalDegreesOfCellsClosestToYou;
            TotalDegreesOfCellsClosestToYou = TotalDegreesOfCellsClosestToOpponent;
            TotalDegreesOfCellsClosestToOpponent = newTotalDegreesOfCellsClosestToOpponent;

            int newTotalDegreesOfCellsReachableByOpponent = TotalDegreesOfCellsReachableByYou;
            TotalDegreesOfCellsReachableByYou = TotalDegreesOfCellsReachableByOpponent;
            TotalDegreesOfCellsReachableByOpponent = newTotalDegreesOfCellsReachableByOpponent;

            int newSumOfDistancesFromYouOnYourClosestCells = SumOfDistancesFromOpponentOnOpponentsClosestCells;
            SumOfDistancesFromOpponentOnOpponentsClosestCells = SumOfDistancesFromYouOnYourClosestCells;
            SumOfDistancesFromYouOnYourClosestCells = newSumOfDistancesFromYouOnYourClosestCells;

            int newSumOfDistancesFromYouOnOpponentsClosestCells = SumOfDistancesFromOpponentOnYourClosestCells;
            SumOfDistancesFromOpponentOnYourClosestCells = SumOfDistancesFromYouOnOpponentsClosestCells;
            SumOfDistancesFromYouOnOpponentsClosestCells = newSumOfDistancesFromYouOnOpponentsClosestCells;

            double newChamberValueForYou = ChamberValueForOpponent;
            ChamberValueForOpponent = ChamberValueForYou;
            ChamberValueForYou = newChamberValueForYou;
        }

        public void RecalculateDegreesOfVertices()
        {
            foreach (CellState cellState in GetAllCellStates())
            {
                cellState.RecalculateDegree();
            }
        }

        public void GoBackToTurn(PlayerType playerWhoMovedLast, int moveNumber, bool performDijkstra = true, bool shouldCalculatedBiconnectedComponents = true)
        {
            if (moveNumber <= 0)
            {
                throw new ArgumentException("Invalid attempt to go back to earlier than turn 1");
            }

            int otherPlayersLastMoveNumber = (playerWhoMovedLast == PlayerWhoMovedFirst) ? moveNumber - 1 : moveNumber;
            PlayerToMoveNext = (playerWhoMovedLast == PlayerType.You) ? PlayerType.Opponent : PlayerType.You;

            int lastMoveNumberForYou = 0;
            int lastMoveNumberForOpponent = 0;

            switch (playerWhoMovedLast)
            {
                case PlayerType.You:
                    lastMoveNumberForYou = moveNumber;
                    lastMoveNumberForOpponent = otherPlayersLastMoveNumber;
                    break;

                case PlayerType.Opponent:
                    lastMoveNumberForYou = otherPlayersLastMoveNumber;
                    lastMoveNumberForOpponent = moveNumber;
                    break;
            }

            foreach (CellState cellState in GetAllCellStates())
            {
                switch (cellState.OccupationStatus)
                {
                    case OccupationStatus.Opponent:
                        if (cellState.MoveNumber > lastMoveNumberForOpponent)
                        {
                            cellState.OccupationStatus = OccupationStatus.Clear;
                        }
                        break;
                    case OccupationStatus.OpponentWall:
                        if (cellState.MoveNumber == lastMoveNumberForOpponent)
                        {
                            cellState.OccupationStatus = OccupationStatus.Opponent;
                            OpponentsCell = cellState;
                        }
                        else
                            if (cellState.MoveNumber > lastMoveNumberForOpponent)
                            {
                                cellState.OccupationStatus = OccupationStatus.Clear;
                            }
                        break;
                    case OccupationStatus.You:
                        if (cellState.MoveNumber > lastMoveNumberForYou)
                        {
                            cellState.OccupationStatus = OccupationStatus.Clear;
                        }
                        break;
                    case OccupationStatus.YourWall:
                        if (cellState.MoveNumber == lastMoveNumberForYou)
                        {
                            cellState.OccupationStatus = OccupationStatus.You;
                            YourCell = cellState;
                        }
                        else
                            if (cellState.MoveNumber > lastMoveNumberForYou)
                            {
                                cellState.OccupationStatus = OccupationStatus.Clear;
                            }
                        break;
                    default:
                        break;
                }
            }

            YourWallLength = lastMoveNumberForYou;
            OpponentsWallLength = lastMoveNumberForOpponent;

            // Update Dijkstra status and distance:
            YourDijkstraStatus = DijkstraStatus.NotCalculated;
            YourUpToDateDijkstraDistance = 0;
            OpponentsDijkstraStatus = DijkstraStatus.NotCalculated;
            OpponentsUpToDateDijkstraDistance = 0;

            RecalculateDegreesOfVertices();

            if (performDijkstra)
            {
                Dijkstra.Perform(this);
            }

            if (shouldCalculatedBiconnectedComponents)
            {
                BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                bcAlg.Calculate(this, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);
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

        #region Biconnected components methods

        public void ClearBiconnectedComponentProperties()
        {
            biconnectedComponents = null;
        }

        public void AddBiconnectedComponent(BiconnectedComponent component)
        {
            if (biconnectedComponents == null)
            {
                biconnectedComponents = new List<BiconnectedComponent>();
            }
            biconnectedComponents.Add(component);
        }

        public IEnumerable<BiconnectedComponent> GetBiconnectedComponents()
        {
            if (biconnectedComponents == null)
            {
                biconnectedComponents = new List<BiconnectedComponent>();
            }
            return biconnectedComponents;
        }

        #endregion

        #region Biconnected chambers methods

        public void ClearChamberPropertiesForPlayer(PlayerType player)
        {
            switch (player)
            {
                case PlayerType.You:
                    ClearYourChamberProperties();
                    break;
                case PlayerType.Opponent:
                    ClearOpponentsChamberProperties();
                    break;
            }
        }

        public void ClearYourChamberProperties()
        {
            yourChambers = null;
        }

        public void ClearOpponentsChamberProperties()
        {
            opponentsChambers = null;
        }

        public void AddChamber(Chamber chamber, PlayerType player)
        {
            switch (player)
            {
                case PlayerType.You:
                    AddChamberForYou(chamber);
                    break;
                case PlayerType.Opponent:
                    AddChamberForOpponent(chamber);
                    break;
            }
        }

        public void AddChamberForYou(Chamber chamber)
        {
            if (yourChambers == null)
            {
                yourChambers = new List<Chamber>();
            }
            yourChambers.Add(chamber);
        }

        public void AddChamberForOpponent(Chamber chamber)
        {
            if (opponentsChambers == null)
            {
                opponentsChambers = new List<Chamber>();
            }
            opponentsChambers.Add(chamber);
        }

        public IEnumerable<Chamber> GetChambersForPlayer(PlayerType player)
        {
            switch (player)
            {
                case PlayerType.You:
                    return GetYourChambers();
                    break;
                default:
                    return GetOpponentsChambers();
                    break;
            }
        }

        public IEnumerable<Chamber> GetYourChambers()
        {
            if (yourChambers == null)
            {
                yourChambers = new List<Chamber>();
            }
            return yourChambers;
        }

        public IEnumerable<Chamber> GetOpponentsChambers()
        {
            if (opponentsChambers == null)
            {
                opponentsChambers = new List<Chamber>();
            }
            return opponentsChambers;
        }

        #endregion

        #region Path-finding algorithms:

        public bool IsCellStateReachableByPlayer(PlayerType player, CellState destinationCellState)
        {
            return (destinationCellState.OccupationStatus == OccupationStatus.Clear
                && (destinationCellState.CompartmentStatus == CompartmentStatus.InYourCompartment
                    || destinationCellState.CompartmentStatus == CompartmentStatus.InSharedCompartment));
        }

        public List<Position> GetPositionsOnAnyRouteToTheTargetPosition(PlayerType player, Position to)
        {
            List<Position> positionsOnRoute = new List<Position>();
            CellState destinationState = this[to];
            if (IsCellStateReachableByPlayer(player, destinationState))
            {
                int distanceFromPlayer = player == PlayerType.You ? destinationState.DistanceFromYou : destinationState.DistanceFromOpponent;

                for (int dist = distanceFromPlayer - 1; dist > 0; dist--)
                {
                    int d = dist;
                    destinationState = destinationState.GetAdjacentCellStates().Where(
                        cs => cs.OccupationStatus == OccupationStatus.Clear && cs.DistanceFromYou == d).FirstOrDefault();
                    positionsOnRoute.Insert(0, destinationState.Position);
                }
            }
            return positionsOnRoute;
        }

        #endregion
    }
}
