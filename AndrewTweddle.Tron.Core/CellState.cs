﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

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

        public GameState GameState
        {
            get
            {
                return gameState;
            }
            private set
            {
                gameState = value;
                OnPropertyChanged("GameState");
            }
        }

        private CellState()
        {
        }

        public CellState(GameState gameState, int x, int y)
        {
            GameState = gameState;
            Position = new Position(x, y);
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
                OnPropertyChanged("Position");
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
                OnPropertyChanged("MoveNumber");
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
                OnPropertyChanged("OccupationStatus");
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
                OnPropertyChanged("DistanceFromYou");
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
                OnPropertyChanged("DistanceFromOpponent");
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
                OnPropertyChanged("ClosestPlayer");
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
                OnPropertyChanged("DegreeOfVertex");
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
                OnPropertyChanged("CompartmentStatus");
            }
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
    }
}
