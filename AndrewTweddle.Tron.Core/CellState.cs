using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AndrewTweddle.Tron.Core
{
    [Serializable]
    public class CellState
    {
        private OccupationStatus occupationStatus;

        public GameState GameState
        {
            get;
            private set;
        }

        private CellState()
        {
        }

        public CellState(GameState gameState, int x, int y)
        {
            GameState = gameState;
            Position = new Position(x, y);
            CellsOnPathToYourCell = new ArrayList();
            CellsOnPathToOpponentsCell = new ArrayList();
        }

        public Position Position { get; private set; }
        public int MoveNumber { get; internal set; }

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
                    occupationStatus = value;

                    switch (occupationStatus)
                    {
                        case OccupationStatus.You:
                            GameState.YourCell = this;
                            break;
                        case OccupationStatus.Opponent:
                            GameState.OpponentsCell = this;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #region Dijkstra algorithm

        public int DistanceFromYou { get; set; }
        public int DistanceFromOpponent { get; set; }
        public PlayerType ClosestPlayer { get; set; }
        public int DegreeOfVertex { get; set; }
        public CompartmentStatus CompartmentStatus { get; set; }
        public ArrayList CellsOnPathToYourCell { get; private set; }
        public ArrayList CellsOnPathToOpponentsCell { get; private set; }

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

            CellsOnPathToYourCell.Clear();
            foreach (CellState previousCellState in source.CellsOnPathToYourCell)
            {
                CellsOnPathToYourCell.Add(GameState[previousCellState.Position]);
            }

            CellsOnPathToOpponentsCell.Clear();
            foreach (CellState previousCellState in source.CellsOnPathToOpponentsCell)
            {
                CellsOnPathToOpponentsCell.Add(GameState[previousCellState.Position]);
            }
        }

        public IEnumerable<CellState> GetAdjacentCellStates()
        {
            return Position.GetAdjacentPositions().Select(pos => GameState[pos]);
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
    }
}
