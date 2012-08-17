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

namespace AndrewTweddle.Tron.Core
{
    public delegate void GameStateEvent(GameState gameState);

    [Serializable]
    public class GameState
    {
        private CellState[,] cells;
        
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

        public PlayerType PlayerWhoMovedFirst { get; private set; }
        public PlayerType PlayerToMoveNext { get; private set; }
        public CellState SouthPole { get; private set; }
        public CellState NorthPole { get; private set; }

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
                if (pos.IsNorthPole)
                {
                    return NorthPole;
                }
                if (pos.IsSouthPole)
                {
                    return SouthPole;
                }
                return cells[pos.X, pos.Y-1];
            }
        }

        public CellState YourCell
        {
            get;
            internal set;
        }

        public CellState OpponentsCell
        {
            get;
            internal set;
        }

        public CellState YourOriginalCell
        {
            get;
            private set;
        }

        public CellState OpponentsOriginalCell
        {
            get;
            private set;
        }

        public int YourWallLength
        {
            get;
            private set;
        }

        public int OpponentsWallLength
        {
            get;
            private set;
        }

        #region Dijkstra and Voronoi information

        public bool OpponentIsInSameCompartment { get; set; }
        public int NumberOfCellsReachableByYou { get; set; }
        public int NumberOfCellsReachableByOpponent { get; set; }
        public int TotalDegreesOfCellsReachableByYou { get; set; }
        public int TotalDegreesOfCellsReachableByOpponent { get; set; }
        public int NumberOfCellsClosestToYou { get; set; }
        public int NumberOfCellsClosestToOpponent { get; set; }
        public int TotalDegreesOfCellsClosestToYou { get; set; }
        public int TotalDegreesOfCellsClosestToOpponent { get; set; }

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

        public void ClearDijkstraProperties()
        {
            OpponentIsInSameCompartment = true;
            NumberOfCellsReachableByYou = 0;
            NumberOfCellsReachableByOpponent = 0;
            TotalDegreesOfCellsReachableByYou = 0;
            TotalDegreesOfCellsReachableByOpponent = 0;
            NumberOfCellsClosestToYou = 0;
            NumberOfCellsClosestToOpponent = 0;
            TotalDegreesOfCellsClosestToYou = 0;
            TotalDegreesOfCellsClosestToOpponent = 0;

            IEnumerable<CellState> cells = GetAllCellStates();
            foreach (CellState cell in cells)
            {
                cell.DistanceFromYou = int.MaxValue;
                cell.DistanceFromOpponent = int.MaxValue;
                cell.ClosestPlayer = PlayerType.Unknown;
                cell.DegreeOfVertex = 0;
                cell.CompartmentStatus = CompartmentStatus.InOtherCompartment;
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

            foreach (CellState source in GetAllCellStates())
            {
                CellState dest = clone[source.Position];
                dest.CopyFrom(source);
            }

            if (OpponentsCell == null)
            {
                clone.OpponentsCell = null;
            }
            else
            {
                clone.OpponentsCell = clone[OpponentsCell.Position];
            }

            if (YourCell == null)
            {
                clone.YourCell = null;
            }
            else
            {
                clone.YourCell = clone[YourCell.Position];
            }

            if (OpponentsOriginalCell == null)
            {
                clone.OpponentsOriginalCell = null;
            }
            else
            {
                clone.OpponentsOriginalCell = clone[OpponentsOriginalCell.Position];
            }

            if (YourOriginalCell == null)
            {
                clone.YourOriginalCell = null;
            }
            else
            {
                clone.YourOriginalCell = clone[YourOriginalCell.Position];
            }

            clone.YourWallLength = YourWallLength;
            clone.OpponentsWallLength = OpponentsWallLength;
            clone.PlayerWhoMovedFirst = PlayerWhoMovedFirst;
            clone.PlayerToMoveNext = PlayerToMoveNext;

            clone.OpponentIsInSameCompartment = OpponentIsInSameCompartment;
            clone.NumberOfCellsReachableByYou = NumberOfCellsReachableByYou;
            clone.NumberOfCellsReachableByOpponent = NumberOfCellsReachableByOpponent;
            clone.TotalDegreesOfCellsReachableByYou = TotalDegreesOfCellsReachableByYou;
            clone.TotalDegreesOfCellsReachableByOpponent = TotalDegreesOfCellsReachableByOpponent;
            clone.NumberOfCellsClosestToYou = NumberOfCellsClosestToYou;
            clone.NumberOfCellsClosestToOpponent = NumberOfCellsClosestToOpponent;
            clone.TotalDegreesOfCellsClosestToYou = TotalDegreesOfCellsClosestToYou;
            clone.TotalDegreesOfCellsClosestToOpponent = TotalDegreesOfCellsClosestToOpponent;

            return clone;
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

        public IEnumerable<GameState> GetPossibleNextStates()
        {
            CellState fromCell = (PlayerToMoveNext == PlayerType.You) ? YourCell : OpponentsCell;

            IEnumerable<CellState> clearCells = fromCell.GetAdjacentCellStates().Where(cs => cs.OccupationStatus == OccupationStatus.Clear);
            foreach (CellState toCell in clearCells)
            {
                GameState nextGameState = Clone();
                nextGameState.MoveToPosition(toCell.Position);
                yield return nextGameState;
            }
        }

        public void MoveToPosition(Position position)
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
            }
            else
            {
                fromCell.OccupationStatus = OccupationStatus.OpponentWall;
                toCell.OccupationStatus = OccupationStatus.Opponent;
                PlayerToMoveNext = PlayerType.You;
                OpponentsWallLength++;
                OpponentsCell = toCell;
            }

            Dijkstra.Perform(this);
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
    }
}
