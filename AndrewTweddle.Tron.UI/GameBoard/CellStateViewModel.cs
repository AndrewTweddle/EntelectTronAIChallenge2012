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
            set
            {
            }
        }

        public int CellWidth
        {
            get
            {
                if (CellState.Position.IsPole)
                {
                    return 25 * 30;
                }
                else
                {
                    return 25;
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
