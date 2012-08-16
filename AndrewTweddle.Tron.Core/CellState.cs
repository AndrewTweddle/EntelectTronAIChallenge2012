using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void CopyFrom(CellState source)
        {
            OccupationStatus = source.OccupationStatus;
            MoveNumber = source.MoveNumber;
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
