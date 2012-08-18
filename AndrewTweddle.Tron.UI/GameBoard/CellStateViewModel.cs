using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using System.Windows.Media;
using System.ComponentModel;

namespace AndrewTweddle.Tron.UI.GameBoard
{
    public class CellStateViewModel: BaseViewModel
    {
        private CellState cellState;

        public GameStateViewModel ParentViewModel
        {
            get;
            private set;
        }

        public CellState CellState
        {
            get
            {
                return cellState;
            }
            set
            {
                cellState = value;
                OnPropertyChanged("CellState");
                OnPropertyChanged("ColumnSpan");
            }
        }

        public int ColumnSpan
        {
            get
            {
                if (CellState != null && CellState.Position.IsPole)
                {
                    return Constants.Columns;
                }
                else
                {
                    return 1;
                }
            }
        }

        public Color BackgroundColor
        {
            get
            {
                if (cellState == null)
                {
                    return Colors.LightGray;
                }
                else
                {
                    switch (cellState.OccupationStatus)
                    {
                        case OccupationStatus.You:
                            return ParentViewModel.YourCellColor;
                        case OccupationStatus.YourWall:
                            return ParentViewModel.YourWallColor;
                        case OccupationStatus.Opponent:
                            return ParentViewModel.OpponentsCellColor;
                        case OccupationStatus.OpponentWall:
                            return ParentViewModel.OpponentsWallColor;
                        default: // OccupationStatus.Clear
                            switch (cellState.ClosestPlayer)
                            {
                                case PlayerType.You:
                                    return ParentViewModel.YourClosestToColor;
                                case PlayerType.Opponent:
                                    return ParentViewModel.OpponentsClosestToColor;
                                default:
                                    return Colors.White;
                            }
                    }
                }
            }
        }

        public int DistanceFromClosestPlayer
        {
            get
            {
                switch (CellState.ClosestPlayer)
                {
                    case PlayerType.You:
                        return CellState.DistanceFromYou;
                    case PlayerType.Opponent:
                        return CellState.DistanceFromOpponent;
                    default:
                        return 0;
                }
            }
        }

        public bool DisplayDistanceFromClosestPlayer
        {
            get
            {
                return (CellState != null) && (CellState.OccupationStatus == OccupationStatus.Clear) 
                    && (CellState.ClosestPlayer != PlayerType.Unknown);
            }
        }

        public CellStateViewModel(GameStateViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            ParentViewModel.PropertyChanged += UpdateCellStateViewModelWhenParentViewModelChanges;
        }

        private void UpdateCellStateViewModelWhenParentViewModelChanges(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName.StartsWith("Your") || e.PropertyName.StartsWith("Opponents"))
                && e.PropertyName.EndsWith("Color"))
            {
                OnPropertyChanged("BackgroundColor");
            }
        }
    }
}
