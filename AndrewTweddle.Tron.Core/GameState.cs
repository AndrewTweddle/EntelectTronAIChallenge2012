using System;
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

        [OptionalField]
        private bool isUsingIncrementalDijkstra;
        
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
            private set
            {
                playerWhoMovedFirst = value;
                OnPropertyChanged("PlayerWhoMovedFirst");
            }
        }

        public PlayerType PlayerToMoveNext
        {
            get
            {
                return playerToMoveNext;
            }
            private set
            {
                playerToMoveNext = value;
                OnPropertyChanged("PlayerToMoveNext");
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
                OnPropertyChanged("SouthPole");
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
                OnPropertyChanged("NorthPole");
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
            internal set
            {
                yourCell = value;
                OnPropertyChanged("YourCell");
            }
        }

        public CellState OpponentsCell
        {
            get
            {
                return opponentsCell;
            }
            internal set
            {
                opponentsCell = value;
                OnPropertyChanged("OpponentsCell");
            }
        }

        public CellState YourOriginalCell
        {
            get
            {
                return yourOriginalCell;
            }
            private set
            {
                yourOriginalCell = value;
                OnPropertyChanged("YourOriginalCell");
            }
        }

        public CellState OpponentsOriginalCell
        {
            get
            {
                return opponentsOriginalCell;
            }
            private set
            {
                opponentsOriginalCell = value;
                OnPropertyChanged("OpponentsOriginalCell");
            }
        }

        public int YourWallLength
        {
            get
            {
                return yourWallLength;
            }
            private set
            {
                yourWallLength = value;
                OnPropertyChanged("YourWallLength");
            }
        }

        public int OpponentsWallLength
        {
            get
            {
                return opponentsWallLength;
            }
            private set
            {
                opponentsWallLength = value;
                OnPropertyChanged("OpponentsWallLength");
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
                OnPropertyChanged("OpponentIsInSameCompartment");
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
                OnPropertyChanged("NumberOfCellsReachableByYou");
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
                OnPropertyChanged("NumberOfCellsReachableByOpponent");
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
                OnPropertyChanged("TotalDegreesOfCellsReachableByYou");
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
                OnPropertyChanged("TotalDegreesOfCellsReachableByOpponent");
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
                OnPropertyChanged("NumberOfCellsClosestToYou");
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
                OnPropertyChanged("NumberOfCellsClosestToOpponent");
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
                OnPropertyChanged("TotalDegreesOfCellsClosestToYou");
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
                OnPropertyChanged("TotalDegreesOfCellsClosestToOpponent");
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
                OnPropertyChanged("YourDijkstraStatus");
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
                OnPropertyChanged("OpponentsDijkstraStatus");
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
                OnPropertyChanged("YourUpToDateDijkstraDistance");
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
                OnPropertyChanged("OpponentsUpToDateDijkstraDistance");
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
                OnPropertyChanged("IsUsingIncrementalDijkstra");
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

        public void LoadRawCellData(IEnumerable<RawCellData> cells)
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
            PlayerToMoveNext = PlayerType.You;

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
                if (opponentsWallLength == yourWallLength + 1)
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

        public void ClearDijkstraProperties(bool updateYourDijkstraProperties = true, bool updateOpponentsDijkstraProperties = true)
        {
            OpponentIsInSameCompartment = true;

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

            IEnumerable<CellState> cells = GetAllCellStates();
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

            IEnumerable<CellState> cells = GetAllCellStates();
            foreach (CellState cell in cells)
            {
                cell.ClearDijkstraStateForPlayer(playerType);
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
            YourDijkstraStatus = sourceGameState.YourDijkstraStatus;
            OpponentsDijkstraStatus = sourceGameState.OpponentsDijkstraStatus;
            YourUpToDateDijkstraDistance = sourceGameState.YourUpToDateDijkstraDistance;
            OpponentsUpToDateDijkstraDistance = sourceGameState.OpponentsUpToDateDijkstraDistance;

            IsUsingIncrementalDijkstra = sourceGameState.IsUsingIncrementalDijkstra;
        }

        public IEnumerable<CellState> GetAllCellStates()
        {
            yield return NorthPole;
            foreach (CellState cellState in cells)
            {
                yield return cellState;
            }
            yield return SouthPole;
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

            IEnumerable<CellState> clearCells = fromCell.GetAdjacentCellStates().Where(cs => cs.OccupationStatus == OccupationStatus.Clear);
            foreach (CellState toCell in clearCells)
            {
                yield return toCell.Position;
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

            IEnumerable<CellState> clearCells = fromCell.GetAdjacentCellStates().Where(cs => cs.OccupationStatus == OccupationStatus.Clear);
            foreach (CellState toCell in clearCells)
            {
                Move move = new Move(PlayerToMoveNext, moveNumber, toCell.Position);
                yield return move;
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

        public void MakeMove(Move move, bool performDijkstra = true)
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
            MoveToPosition(move.To, performDijkstra);
        }

        public void MoveToPosition(Position position, bool performDijkstra = true)
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

        public static GameState InitializeNewGameState(bool performDijkstra = true)
        {
            GameState gameState = new GameState();
            Random rnd = new Random();
            int yourX = rnd.Next(Constants.Columns);
            int yourY = rnd.Next(Constants.Rows - 2) + 1;
            int opponentsX = (yourX + Constants.Columns / 2) % Constants.Columns;
            int opponentsY = yourY; // was: Constants.SouthPoleY - yourY;
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
