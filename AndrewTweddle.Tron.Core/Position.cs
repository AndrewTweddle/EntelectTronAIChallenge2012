using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    [Serializable]
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return 31 * X + Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position)
            {
                Position otherPosition = (Position)obj;
                return otherPosition.X == X && otherPosition.Y == Y;
            }
            return false;
        }

        public bool IsNorthPole
        {
            get
            {
                return Y == Constants.NorthPoleY;
            }
        }

        public bool IsSouthPole
        {
            get
            {
                return Y == Constants.SouthPoleY;
            }
        }

        public bool IsPole
        {
            get
            {
                return IsNorthPole || IsSouthPole;
            }
        }

        public IEnumerable<Position> GetAdjacentPositions()
        {
            if (IsPole)
            {
                int y = IsNorthPole ? Constants.ArcticCircleY : Constants.AntarcticCircleY;
                for (int x = 0; x < 29; x++)
                {
                    yield return new Position(x, y);
                }
            }
            else
            {
                // West:
                yield return new Position((X + Constants.Columns - 1) % Constants.Columns, Y);

                // East:
                yield return new Position((X + 1) % Constants.Columns, Y);

                // North:
                if (Y == Constants.ArcticCircleY)
                {
                    yield return new Position(Constants.NorthPoleX, Constants.NorthPoleY);
                }
                else
                {
                    yield return new Position(X, Y - 1);
                }

                // South:
                if (Y == Constants.AntarcticCircleY)
                {
                    yield return new Position(Constants.SouthPoleX, Constants.SouthPoleY);
                }
                else
                {
                    yield return new Position(X, Y + 1);
                }
            }
        }
    }
}
