using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    [Serializable]
    public class Position: INotifyPropertyChanged
    {
        private int x;
        private int y;

        [NonSerialized]
        private int hashCode;

        [NonSerialized]
        private static readonly Position[,][] adjacentPositions = new Position[Constants.Columns,Constants.Rows][];

        [NonSerialized]
        private static readonly int[,] initialDegreesOfVertices = new int[Constants.Columns, Constants.Rows];

        public int X 
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                OnPropertyChanged("X");
            }
        }

        public int Y 
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                OnPropertyChanged("Y");
            }
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        static Position()
        {
            /* Cache adjacent positions to speed up enumeration: */
            Position newPosition;

            for (int X = 0; X < Constants.Columns; X++)
            {
                for (int Y = 0; Y < Constants.Rows; Y++)
                {
                    List<Position> adjacentPositionsList = new List<Position>();

                    if (Y == Constants.SouthPoleY || Y == Constants.NorthPoleY)
                    {
                        int y = (Y == Constants.NorthPoleY) ? Constants.ArcticCircleY : Constants.AntarcticCircleY;
                        for (int x = 0; x < Constants.Columns; x++)
                        {
                            newPosition = new Position(x, y);
                            adjacentPositionsList.Add(newPosition);
                        }
                    }
                    else
                    {
                        // West:
                        newPosition = new Position((X + Constants.Columns - 1) % Constants.Columns, Y);
                        adjacentPositionsList.Add(newPosition);

                        // East:
                        newPosition = new Position((X + 1) % Constants.Columns, Y);
                        adjacentPositionsList.Add(newPosition);

                        // North:
                        if (Y == Constants.ArcticCircleY)
                        {
                            newPosition = new Position(Constants.NorthPoleX, Constants.NorthPoleY);
                            adjacentPositionsList.Add(newPosition);
                        }
                        else
                        {
                            newPosition = new Position(X, Y - 1);
                            adjacentPositionsList.Add(newPosition);
                        }

                        // South:
                        if (Y == Constants.AntarcticCircleY)
                        {
                            newPosition = new Position(Constants.SouthPoleX, Constants.SouthPoleY);
                            adjacentPositionsList.Add(newPosition);
                        }
                        else
                        {
                            newPosition = new Position(X, Y + 1);
                            adjacentPositionsList.Add(newPosition);
                        }
                    }

                    Position[] adjacentPositionsArray = adjacentPositionsList.ToArray();
                    adjacentPositions[X, Y] = adjacentPositionsArray;
                    initialDegreesOfVertices[X, Y] = adjacentPositionsArray.Length;
                }
            }
        }

        public override int GetHashCode()
        {
            if (hashCode == 0)
            {
                if (Y == Constants.SouthPoleY || Y == Constants.NorthPoleY)
                {
                    hashCode = Y + 1;
                }
                else
                {
                    hashCode = 31 * X + Y + 1;
                }
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position && obj != null)
            {
                Position otherPosition = (Position)obj;
                if (Y == Constants.SouthPoleY || Y == Constants.NorthPoleY)
                {
                    return otherPosition.Y == Y;
                }
                else
                {
                    return otherPosition.Y == Y && otherPosition.X == X;
                }
            }
            return false;
        }

        public static bool operator ==(Position position1, Position position2)
        {
            if (object.ReferenceEquals(position1, null))
            {
                if (object.ReferenceEquals(position2, null))
                {
                    return true;
                }
                return false;
            }
            return position1.Equals(position2);
        }

        public static bool operator !=(Position position1, Position position2)
        {
            return !(position1 == position2);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public bool IsNorthPole
        {
            get
            {
                return y == Constants.NorthPoleY;
            }
        }

        public bool IsSouthPole
        {
            get
            {
                return y == Constants.SouthPoleY;
            }
        }

        public bool IsPole
        {
            get
            {
                return IsNorthPole || IsSouthPole;
            }
        }

        public Position[] GetAdjacentPositions()
        {
            return adjacentPositions[X, Y];
        }

        public int GetInitialDegreeOfVertex()
        {
            return initialDegreesOfVertices[X, Y];
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
