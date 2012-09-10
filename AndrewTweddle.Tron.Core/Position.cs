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
#if DEBUG
                OnPropertyChanged("X");
#endif
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
#if DEBUG
                OnPropertyChanged("Y");
#endif
            }
        }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        static Position()
        {
            /* Cache adjacent positions to speed up enumeration: */
            Position newPosition;

            for (int xiter = 0; xiter < Constants.Columns; xiter++)
            {
                for (int yiter = 0; yiter < Constants.Rows; yiter++)
                {
                    List<Position> adjacentPositionsList = new List<Position>();

                    if (yiter == Constants.SouthPoleY || yiter == Constants.NorthPoleY)
                    {
                        int y = (yiter == Constants.NorthPoleY) ? Constants.ArcticCircleY : Constants.AntarcticCircleY;
                        for (int x = 0; x < Constants.Columns; x++)
                        {
                            newPosition = new Position(x, y);
                            adjacentPositionsList.Add(newPosition);
                        }
                    }
                    else
                    {
                        // West:
                        newPosition = new Position((xiter + Constants.Columns - 1) % Constants.Columns, yiter);
                        adjacentPositionsList.Add(newPosition);

                        // East:
                        newPosition = new Position((xiter + 1) % Constants.Columns, yiter);
                        adjacentPositionsList.Add(newPosition);

                        // North:
                        if (yiter == Constants.ArcticCircleY)
                        {
                            newPosition = new Position(Constants.NorthPoleX, Constants.NorthPoleY);
                            adjacentPositionsList.Add(newPosition);
                        }
                        else
                        {
                            newPosition = new Position(xiter, yiter - 1);
                            adjacentPositionsList.Add(newPosition);
                        }

                        // South:
                        if (yiter == Constants.AntarcticCircleY)
                        {
                            newPosition = new Position(Constants.SouthPoleX, Constants.SouthPoleY);
                            adjacentPositionsList.Add(newPosition);
                        }
                        else
                        {
                            newPosition = new Position(xiter, yiter + 1);
                            adjacentPositionsList.Add(newPosition);
                        }
                    }

                    Position[] adjacentPositionsArray = adjacentPositionsList.ToArray();
                    adjacentPositions[xiter, yiter] = adjacentPositionsArray;
                    initialDegreesOfVertices[xiter, yiter] = adjacentPositionsArray.Length;
                }
            }
        }

        public override int GetHashCode()
        {
            if (hashCode == 0)
            {
                if (y == Constants.SouthPoleY || y == Constants.NorthPoleY)
                {
                    hashCode = y + 1;
                }
                else
                {
                    hashCode = 31 * x + y + 1;
                }
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position && obj != null)
            {
                Position otherPosition = (Position)obj;
                if (y == Constants.SouthPoleY || y == Constants.NorthPoleY)
                {
                    return otherPosition.y == y;
                }
                else
                {
                    return otherPosition.y == y && otherPosition.x == x;
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
            return String.Format("({0}, {1})", x, y);
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
            return adjacentPositions[x, y];
        }

        public int GetInitialDegreeOfVertex()
        {
            return initialDegreesOfVertices[x, y];
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
