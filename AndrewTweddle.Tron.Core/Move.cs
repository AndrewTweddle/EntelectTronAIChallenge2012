using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public class Move
    {
        public PlayerType PlayerType { get; private set; }
        public int MoveNumber { get; private set; }
        public Position To { get; private set; }

        public Move(PlayerType playerType, int moveNumber, Position to)
        {
            PlayerType = playerType;
            MoveNumber = moveNumber;
            To = to;
        }

        public override bool Equals(object obj)
        {
            if (obj is Move)
            {
                Move otherMove = (Move)obj;
                return otherMove.PlayerType == PlayerType && otherMove.MoveNumber == MoveNumber && otherMove.To == To;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)PlayerType ^ MoveNumber ^ To.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("move to {0} by {1} on turn {2}", To, PlayerType, MoveNumber);
        }
    }
}
